using System.Collections.Generic;
using Service.Balances.Domain.Models;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.WalletBalances)]
    public class WalletBalancesMessage
    {
        public List<WalletBalance> Balances { get; set; }
    }
}