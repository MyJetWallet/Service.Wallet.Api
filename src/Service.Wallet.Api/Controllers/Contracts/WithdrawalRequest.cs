namespace Service.Wallet.Api.Controllers.Contracts
{
    public class WithdrawalRequest
    {
        public string RequestId { get; set; }

        public string WalletId { get; set; }

        public string AssetSymbol { get; set; }

        public double Amount { get; set; }

        public string ToAddress { get; set; }
    }
}