using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Domain;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Domain.Wallets
{
    public class WalletService : IWalletService
    {

        public ValueTask<List<ClientWallet>> GetWalletsAsync(IJetClientIdentity clientId)
        {
            var result = new List<ClientWallet>()
            {
                new ClientWallet()
                {
                    WalletId = $"{clientId.ClientId}--default",
                    Name = "Wallet",
                    IsDefault = true
                }
            };

            return new ValueTask<List<ClientWallet>>(result);
        }

        public ValueTask<ClientWallet> GetDefaultWalletAsync(IJetClientIdentity clientId)
        {
            var result = new ClientWallet()
            {
                WalletId = $"{clientId.ClientId}--default",
                Name = "Wallet",
                IsDefault = true
            };

            return new ValueTask<ClientWallet>(result);
        }

        public ValueTask<ClientWallet> GetWalletByIdAsync(IJetClientIdentity clientId, string walletId)
        {
            if (walletId == $"{clientId.ClientId}--default")
            {
                var result = new ClientWallet()
                {
                    WalletId = $"{clientId.ClientId}--default",
                    Name = "Wallet",
                    IsDefault = true
                };

                return new ValueTask<ClientWallet>(result);
            }

            return new ValueTask<ClientWallet>((ClientWallet)null);
        }

        public async ValueTask<JetWalletIdentity> GetWalletIdentityByIdAsync(IJetClientIdentity clientId, string walletId)
        {
            var wallet = await GetWalletByIdAsync(clientId, walletId);

            return new JetWalletIdentity(clientId.BrokerId, clientId.BrandId, clientId.ClientId, wallet.WalletId);
        }
    }
}