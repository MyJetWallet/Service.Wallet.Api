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
using Service.Registration.Grpc;
using Service.Registration.Grpc.Models;
using Service.Wallet.Api.Controllers;

namespace Service.Wallet.Api.Authentication
{
    public class WalletAuthHandler : AuthenticationHandler<WalletAuthenticationOptions>
    {
        private readonly IClientRegistrationService _clientRegistrationService;
        public const string DefaultBroker = "jetwallet";
        public const string DefaultBrand = "default-brand";
        public const string ClientIdClaim = "ClientId";

        public WalletAuthHandler(IOptionsMonitor<WalletAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            IClientRegistrationService clientRegistrationService) : base(options, logger, encoder, clock)
        {
            _clientRegistrationService = clientRegistrationService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var brokerId = DefaultBroker;

                //todo: extract brand some how
                var brandId = DefaultBrand;
                
                var traderId = Context.GetTraderId();

                var clientId = new JetClientIdentity(brokerId, brandId, traderId);

                var response = await _clientRegistrationService.GetOrRegisterClientAsync(clientId);

                if (response.Result != ClientRegistrationResponse.RegistrationResult.Ok)
                {
                    Logger.LogError("Cannot register client. Client already register with another brand. BrokerId/BrandId/ClientId: {brokerId}/{brandId}/{clientId}",
                        clientId.BrokerId, clientId.BrandId, clientId.ClientId);

                    throw new UnauthorizedAccessException("Cannot register client. Client already register with another brand.");
                }

                var identity = new GenericIdentity(traderId);
                identity.AddClaim(new Claim(ClientIdClaim, JsonConvert.SerializeObject(clientId)));
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (UnauthorizedAccessException ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }
            catch (Exception)
            {
                return AuthenticateResult.Fail("unauthorized");
            }
        }
    }
}