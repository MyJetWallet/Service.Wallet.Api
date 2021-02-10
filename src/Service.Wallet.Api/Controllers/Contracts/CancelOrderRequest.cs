namespace Service.Wallet.Api.Controllers.Contracts
{
    public class CancelOrderRequest : WalletRequest
    {
        public string OrderId { get; set; }
    }
}