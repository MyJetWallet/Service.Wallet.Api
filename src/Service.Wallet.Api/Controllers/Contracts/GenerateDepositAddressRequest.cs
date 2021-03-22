namespace Service.Wallet.Api.Controllers.Contracts
{
    public class GenerateDepositAddressRequest
    {
        public string WalletId { get; set; }

        public string AssetSymbol { get; set; }
    }
}