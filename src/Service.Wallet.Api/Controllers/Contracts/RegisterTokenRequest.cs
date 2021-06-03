namespace Service.Wallet.Api.Controllers.Contracts
{
    public class RegisterTokenRequest
    {
        public string Token { get; set; }

        public string UserLocale { get; set; }
    }
}