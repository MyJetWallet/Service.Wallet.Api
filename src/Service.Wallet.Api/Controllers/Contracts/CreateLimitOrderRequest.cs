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

        public Direction Direction { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }
    }
}