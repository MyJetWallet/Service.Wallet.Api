using System;

namespace Service.Wallet.Api.Domain.Models
{
    public class BalanceHistoryItem
    {
        public string AssetSymbol { get; set; }

        public decimal Amount { get; set; }

        public DateTime Timestamp { get; set; }

        public BalanceHistoryType Type { get; set; }
    }
}