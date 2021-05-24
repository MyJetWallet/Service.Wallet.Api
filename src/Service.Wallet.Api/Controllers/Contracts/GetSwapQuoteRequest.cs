using MyJetWallet.Domain.Orders;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class GetSwapQuoteRequest : WalletRequest
    {
        public string InstrumentSymbol { get; set; }
        
        public string AssetSymbol { get; set; }

        public OrderSide Side { get; set; }

        public double Volume { get; set; }
    }
}