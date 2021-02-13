using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyJetWallet.Domain;
using Newtonsoft.Json;
using Service.Wallet.Api.Controllers;

namespace Service.Wallet.Api.Authentication
{
    public class WalletAuthHandler : AuthenticationHandler<WalletAuthenticationOptions>
    {
        public const string DefaultBroker = "jetwallet";
        public const string DefaultBrand = "default-brand";
        public const string ClientIdClaim = "ClientId";

        public WalletAuthHandler(IOptionsMonitor<WalletAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var brokerId = DefaultBroker;

                //todo: extract brand some how
                var brandId = DefaultBrand;
                
                var traderId = Context.GetTraderId();

                var clientId = new JetClientIdentity(brokerId, brandId, traderId);

                var identity = new GenericIdentity(traderId);
                identity.AddClaim(new Claim(ClientIdClaim, JsonConvert.SerializeObject(clientId)));
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Task.FromResult(AuthenticateResult.Fail(ex.Message));
            }
            catch (Exception)
            {
                return Task.FromResult(AuthenticateResult.Fail("unauthorized"));
            }
        }
    }
}