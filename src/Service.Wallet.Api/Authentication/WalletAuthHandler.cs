using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Wallet.Api.Controllers;

namespace Service.Wallet.Api.Authentication
{
    public class WalletAuthHandler : AuthenticationHandler<WalletAuthenticationOptions>
    {
        public WalletAuthHandler(IOptionsMonitor<WalletAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var traderId = Context.GetTraderId();
                var identity = new GenericIdentity(traderId);
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