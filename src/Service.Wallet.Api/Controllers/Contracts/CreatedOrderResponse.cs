using MyJetWallet.Domain.Orders;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class CreatedOrderResponse
    {
        public OrderType Type { get; set; }

        public string OrderId { get; set; }

        public double OrderPrice { get; set; }
    }
}