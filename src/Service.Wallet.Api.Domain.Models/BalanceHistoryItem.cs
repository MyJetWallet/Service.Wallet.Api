using System;

namespace Service.Wallet.Api.Domain.Models
{
    public class BalanceHistoryItem
    {
        public string AssetSymbol { get; set; }

        public double Amount { get; set; }

        public DateTime Timestamp { get; set; }

        public BalanceHistoryType Type { get; set; }
        
        public long SequenceId { get; set; }
    }
}