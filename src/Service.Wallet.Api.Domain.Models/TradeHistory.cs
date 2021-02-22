using System;
using MyJetWallet.Domain.Orders;

namespace Service.Wallet.Api.Domain.Models
{
    public class TradeHistory
    {
        public string WalletId { get; set; }

        public string TradeId { get; set; }

        public string InstrumentSymbol { get; set; }

        public decimal Volume { get; set; }

        public decimal Price { get; set; }

        public decimal QuoteVolume { get; set; }

        public OrderSide Side { get; set; }

        public DateTime Timestamp { get; set; }

        public string OrderId { get; set; }

        public OrderType Type { get; set; }
    }
}