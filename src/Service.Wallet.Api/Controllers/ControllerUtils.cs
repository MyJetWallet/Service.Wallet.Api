using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using Service.Wallet.Api.Authentication;
using Service.Wallet.Api.Domain.Contracts;
using Service.Wallet.Api.Domain.Wallets;
using SimpleTrading.TokensManager;
using SimpleTrading.TokensManager.Tokens;

namespace Service.Wallet.Api.Controllers
{
    public static class ControllerUtils
    {
        public static IWalletService WalletService { get; set; }

        /// <summary>
        /// PrintToken
        /// </summary>
        /// <param name="tokenString"></param>
        /// <returns></returns>
        public static string PrintToken(this string tokenString)
        {
            try
            {
                var (result, token) = TokensManager.ParseBase64Token<AuthorizationToken>(tokenString, Startup.SessionEncodingKey, DateTime.UtcNow);

                return JsonConvert.SerializeObject(new
                {
                    TraderId = token.Id,
                    Expires = token.Expires.ToString("s"),
                });
            }
            catch (Exception)
            {
                return "Invalid token";
            }
        }

        private static readonly string[] IpHeaders =
        {
            "CF-Connecting-IP", "X-Forwarded-For"
        };

        private static string GetIp(this HttpRequest httpRequest)
        {
            foreach (var ipHeader in IpHeaders)
            {
                if (httpRequest.Headers.ContainsKey(ipHeader))
                    return httpRequest.Headers[ipHeader].ToString();
            }

            return httpRequest?.HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// Get Ip of request
        /// </summary>
        /// <param name="ctx">Request context</param>
        /// <returns></returns>
        public static string GetIp(this HttpContext ctx)
        {
            return ctx.Request.GetIp();
        }

        /// <summary>
        /// User agent of request
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string GetUserAgent(this HttpContext ctx)
        {
            try
            {
                return ctx.Request.Headers["User-Agent"];
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get scheme of request
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string GetScheme(this HttpContext ctx)
        {
            const string headerName = "X-Forwarded-Proto";
            if (ctx.Request.Headers.ContainsKey(headerName))
                return ctx.Request.Headers[headerName];
            return ctx.Request.Scheme;
        }


        private const string AuthorizationHeader = "authorization";

        /// <summary>
        /// Get trader id of request
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException">Throws if token is expired, wrong or does not exist</exception>
        public static string GetTraderId(this HttpContext ctx)
        {
            if (!ctx.Request.Headers.ContainsKey(AuthorizationHeader))
                throw new UnauthorizedAccessException("UnAuthorized request");

            var itm = ctx.Request.Headers[AuthorizationHeader].ToString().Trim();
            var items = itm.Split();
            return items[^1].GetTraderIdByToken();

        }

        /// <summary>
        /// GetTraderIdByToken
        /// </summary>
        /// <param name="tokenString"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public static string GetTraderIdByToken(this string tokenString)
        {
            //todo: add authorization by token
            try
            {
                //var (result, token) = TokensManager.TokensManager.ParseBase64Token<AuthorizationToken>(tokenString, ServiceLocator.SessionEncodingKey, DateTime.UtcNow);

                //if (result == TokenParseResult.Expired)
                //    throw new UnauthorizedAccessException("UnAuthorized request");

                //if (result == TokenParseResult.InvalidToken)
                //    throw new UnauthorizedAccessException("UnAuthorized request");

                //if (tokenString != "alex-test")
                //    throw new UnauthorizedAccessException("UnAuthorized request");

                return tokenString;
            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException("UnAuthorized request");
            }
        }

        public static ValueTask<IJetWalletIdentity> GetWalletIdentityAsync(this HttpContext context, string walletId)
        {
            var clientId = context.GetClientIdentity();

            return context.GetWalletIdentityAsync(walletId, clientId);
        }

        private static async ValueTask<IJetWalletIdentity> GetWalletIdentityAsync(this HttpContext context, string walletId, JetClientIdentity clientId)
        {
            if (string.IsNullOrEmpty(walletId))
                throw new WalletApiBadRequestException("Wallet cannot be empty");

            var wallet = await WalletService.GetWalletIdentityByIdAsync(clientId, walletId);

            if (wallet == null)
            {
                throw new WalletApiErrorException("Wallet do not found", ApiResponseCodes.WalletDoNotExist);
            }

            wallet.WalletId.AddToActivityAsTag("walletId");
            Activity.Current?.AddBaggage("wallet-id", walletId);

            return wallet;
        }

        public const string DefaultBroker = "jetwallet";

        public static JetClientIdentity GetClientIdentity(this HttpContext context)
        {
            var clientIdClaim = context?.User?.Claims.FirstOrDefault(e => e.Type == WalletAuthHandler.ClientIdClaim);

            if (string.IsNullOrEmpty(clientIdClaim?.Value))
            {
                throw new WalletApiBadRequestException("Cannot extract user from request context.");
            }

            var clientId = JsonConvert.DeserializeObject<JetClientIdentity>(clientIdClaim.Value);

            return clientId;
        }

        public static JetClientIdentity GetClientIdByToken(this HttpContext context, string token)
        {
            var clientId = token.GetTraderIdByToken();

            return new JetClientIdentity(WalletAuthHandler.DefaultBroker, WalletAuthHandler.DefaultBrand, clientId);
        }
    }
}