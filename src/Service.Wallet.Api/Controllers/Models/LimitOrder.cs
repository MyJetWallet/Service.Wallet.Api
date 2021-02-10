using System;

namespace Service.Wallet.Api.Controllers.Models
{
    public class LimitOrder
    {
        public string WalletId { get; set; }

        public string OrderId { get; set; }

        public OrderType Type { get; set; }

        public string InstrumentSymbol { get; set; }

        public Direction Direction { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }

        public decimal FilledVolume { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdate { get; set; }

        public OrderStatus Status { get; set; }
    }
}