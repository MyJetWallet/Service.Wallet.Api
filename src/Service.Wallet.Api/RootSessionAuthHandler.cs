using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyJetWallet.Sdk.Authorization;
using MyJetWallet.Sdk.Authorization.Http;
using MyJetWallet.Sdk.Authorization.NoSql;
using MyNoSqlServer.Abstractions;
using OpenTelemetry.Trace;
using SimpleTrading.ClientApi.Utils;
using SimpleTrading.TokensManager;
using SimpleTrading.TokensManager.Tokens;

namespace Service.Wallet.Api
{
    public class RootSessionAuthHandler11 : AuthenticationHandler<MyAuthenticationOptions>
    {
        public static TimeSpan SessionTrustedTimeSpan = TimeSpan.FromSeconds(60);
        public static bool IsDevelopmentEnvironment = false;

        private readonly IMyNoSqlServerDataReader<ShortRootSessionNoSqlEntity> _reader;
        

        public RootSessionAuthHandler11(IOptionsMonitor<MyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            IMyNoSqlServerDataReader<ShortRootSessionNoSqlEntity> reader) : base(options, logger, encoder, clock)
        {
            _reader = reader;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var activity = Activity.Current;

            try
            {
                if (!Context.Request.Headers.ContainsKey(AuthorizationConst.AuthorizationHeader))
                    throw new UnauthorizedAccessException("UnAuthorized request");


                if (Request.Method == "POST")
                {
                    Request.EnableBuffering();

                    string bodyStr;
                    using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true))
                    {
                        bodyStr = await reader.ReadToEndAsync();
                        Console.WriteLine(bodyStr);
                    }

                    Request.Body.Position = 0;
                }


                var itm = Context.Request.Headers[AuthorizationConst.AuthorizationHeader].ToString().Trim();
                var items = itm.Split();
                var authToken = items[^1];

                var (result, token) = TokensManager.ParseBase64Token<AuthorizationToken>(authToken, AuthorizationConst.GetSessionEncodingKey(), DateTime.UtcNow);

                activity?.AddTag("clientId", token?.Id);

                if (result != TokenParseResult.Ok)
                {
                    throw new UnauthorizedAccessException($"Wrong token: {result.ToString()}");
                }

                if (token == null)
                {
                    throw new UnauthorizedAccessException($"Wrong token: cannot parse token");
                }

                if (string.IsNullOrEmpty(token.Id) || !token.RootSessionId.HasValue || token.Expires < DateTime.UtcNow)
                {
                    throw new UnauthorizedAccessException($"Wrong token: not valid");
                }

                if (!string.IsNullOrEmpty(token.Ip) && !IsDevelopmentEnvironment)
                {
                    var currentIp = Context.GetIp();

                    if (currentIp != token.Ip)
                    {
                        throw new UnauthorizedAccessException($"Wrong token: IP restriction");
                    }
                }

                activity?.AddTag("auth-rootId", token.RootSessionId.Value);
                activity?.AddTag("brokerId", AuthorizationConst.DefaultBrokerId);
                activity?.AddTag("brandId", token.BrandId);
                activity?.AddTag("auth-tokenId", token.TokenId);

                ValidateRootSession(token);

                var identity = new GenericIdentity(token.Id);
                identity.AddClaim(new Claim(AuthorizationConst.ClientIdClaim, token.Id));
                identity.AddClaims(new []
                {
                    new Claim(AuthorizationConst.ClientIdClaim, token.Id),
                    new Claim(AuthorizationConst.BrokerIdClaim, AuthorizationConst.DefaultBrokerId),
                    new Claim(AuthorizationConst.BrandIdClaim, token.BrandId),
                    new Claim(AuthorizationConst.SessionRootIdClaim, token.RootSessionId.Value.ToString("N")),
                    new Claim(AuthorizationConst.SessionTokenIdClaim, token.TokenId.ToString())
                });

                if (token.PublicKey?.Any() == true)
                {
                    var signVerified = await ValidateSignature(token, Context.Request);
                    if (signVerified)
                        identity.AddClaim(new Claim(AuthorizationConst.SignatureVerifiedClaim, "true"));
                }

                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (UnauthorizedAccessException ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(Status.Error);
                return AuthenticateResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(Status.Error);
                return AuthenticateResult.Fail("unauthorized");
            }
        }

        private void ValidateRootSession(AuthorizationToken token)
        {
            if (!token.RootSessionId.HasValue)
            {
                throw new UnauthorizedAccessException($"Wrong token: root session is not found"); 
            }

            var rootSession = _reader.Get(ShortRootSessionNoSqlEntity.GeneratePartitionKey(token.Id), ShortRootSessionNoSqlEntity.GenerateRowKey(token.RootSessionId.Value));

            if (rootSession == null)
            {
                if (DateTime.UtcNow - token.CreatedTime() > SessionTrustedTimeSpan)
                {
                    throw new UnauthorizedAccessException($"Wrong token: root session is not found");
                }
            }

            if (string.IsNullOrEmpty(token.BrandId))
            {
                throw new UnauthorizedAccessException($"Wrong token: brandId is empty");
            }
        }

        private async Task<bool> ValidateSignature(AuthorizationToken token, HttpRequest request)
        {
            if (request.Method != "POST" || 
                !request.Headers.TryGetValue(AuthorizationConst.SignatureHeader, out var signature) || 
                token.PublicKey?.Any() != true)
            { 
                return false;
            }

            Console.WriteLine($"Position before: {request.Body.Position}");

            request.EnableBuffering();

            string bodyStr;
            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = await reader.ReadToEndAsync();
            }

            request.Body.Position = 0;

            var isValidSignature = MyControllerBaseHelper.ValidateSignature(bodyStr, signature, token.PublicKey);

            if (!isValidSignature)
            {
                Console.WriteLine($"Body: {bodyStr}\nSignature: {signature}\nPublic key: {Convert.ToBase64String(token.PublicKey)}");
                throw new UnauthorizedAccessException($"Wrong signature");
            }

            return true;
        }
    }
}