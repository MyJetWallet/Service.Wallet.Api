namespace Service.Wallet.Api.Controllers.Contracts
{
    public class WithdrawalResponse
    {
        public string OperationId { get; set; }

        public string TxId { get; set; }

        public string TxUrl { get; set; }
    }
}