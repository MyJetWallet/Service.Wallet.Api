using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Domain;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Domain.Wallets
{
    public interface IWalletService
    {
        ValueTask<List<ClientWallet>> GetWalletsAsync(IJetClientIdentity clientId);

        ValueTask<ClientWallet> GetDefaultWalletAsync(IJetClientIdentity clientId);

        ValueTask<ClientWallet> GetWalletByIdAsync(IJetClientIdentity clientId, string walletId);

        ValueTask<JetWalletIdentity> GetWalletIdentityByIdAsync(IJetClientIdentity clientId, string walletId);
    }
}