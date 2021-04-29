namespace Service.Wallet.Api.Controllers.Contracts
{
    public class AuthorizationRequest
    {
        public string AuthToken { get; set; }

        public string PublicKeyPem { get; set; }
    }
}