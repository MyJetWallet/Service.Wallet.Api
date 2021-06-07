using System.Collections.Generic;
using Service.BalanceHistory.Domain.Models;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.Trades)]
    public class TradesMessage
    {
        public string WalletId { get; set; }

        public List<WalletTrade> Trades { get; set; }
    }
}