namespace Service.Wallet.Api.Controllers.Contracts
{
    public class ValidationAddressRequest
    {
        public string AssetSymbol { get; set; }

        public string ToAddress { get; set; }
    }
}