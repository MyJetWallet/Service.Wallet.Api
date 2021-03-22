namespace Service.Wallet.Api.Controllers.Contracts
{
    public class GenerateDepositAddressResponse
    {
        public string Address { get; set; }

        public string Memo { get; set; }

        public string MemoType { get; set; }
    }
}