using System.Collections.Generic;
using Service.ClientWallets.Domain.Models;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.WalletList)]
    public class WalletListMessage: MesssageContract
    {
        public List<ClientWallet> Wallets { get; set; }
    }
}