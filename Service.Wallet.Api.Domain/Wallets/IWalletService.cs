using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Domain; 
using Service.ClientWallets.Domain.Models;

namespace Service.Wallet.Api.Domain.Wallets
{
    public interface IWalletService
    {
        ValueTask<List<ClientWallet>> GetWalletsAsync(JetClientIdentity clientId);

        ValueTask<ClientWallet> GetDefaultWalletAsync(JetClientIdentity clientId);

        ValueTask<ClientWallet> GetWalletByIdAsync(JetClientIdentity clientId, string walletId);

        ValueTask<JetWalletIdentity> GetWalletIdentityByIdAsync(JetClientIdentity clientId, string walletId);
    }
}