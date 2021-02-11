using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class CreateMarketOrderRequest : WalletRequest
    {
        public string InstrumentSymbol { get; set; }

        public Direction Direction { get; set; }

        public decimal Volume { get; set; }
    }
}