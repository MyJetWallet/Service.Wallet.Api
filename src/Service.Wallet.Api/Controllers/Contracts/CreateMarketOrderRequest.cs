using MyJetWallet.Domain.Orders;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class CreateMarketOrderRequest : WalletRequest
    {
        public string InstrumentSymbol { get; set; }

        public OrderSide Side { get; set; }

        public double Volume { get; set; }
    }
}