namespace Service.Wallet.Api.Controllers.Contracts
{
    public class GetSessionInfoResponse
    {
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public bool TwoFactorAuthentication { get; set; }
        public string TokenLifetimeRemaining { get; set; }
    }
}