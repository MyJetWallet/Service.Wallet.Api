using MyJetWallet.Domain.Orders;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class CreateSwapOrderRequest : WalletRequest
    {
        public string InstrumentSymbol { get; set; }

        public OrderSide Side { get; set; }

        public double Volume { get; set; }

        public string VolumeAssetSymbol { get; set; }
    }
}