namespace Service.Wallet.Api.Controllers.Contracts
{
    public class ValidationAddressRequest
    {
        public string WalletId { get; set; }

        public string AssetSymbol { get; set; }

        public string ToAddress { get; set; }
    }
}