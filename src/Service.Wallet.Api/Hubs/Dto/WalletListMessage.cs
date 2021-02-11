using System.Collections.Generic;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.WalletList)]
    public class WalletListMessage
    {
        public List<ClientWallet> Wallets { get; set; }
    }
}