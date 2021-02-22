using MyJetWallet.Domain.Orders;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Controllers.Contracts
{
    /// <summary>
    /// Request to create limit order
    /// </summary>
    public class CreateLimitOrderRequest : WalletRequest
    {
        /// <summary>
        /// Unique symbol of trading instrument
        /// </summary>
        public string InstrumentSymbol { get; set; }

        public OrderSide Side { get; set; }

        public double Price { get; set; }

        public double Volume { get; set; }
    }
}