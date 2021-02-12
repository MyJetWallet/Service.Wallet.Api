using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using Service.ClientWallets.Domain.Models;
using Service.ClientWallets.Grpc;

namespace Service.Wallet.Api.Domain.Wallets
{
    public class WalletService : IWalletService
    {
        private readonly IClientWalletService _clientWalletService;
        private readonly ILogger<WalletService> _logger;

        public WalletService(IClientWalletService clientWalletService, ILogger<WalletService> logger)
        {
            _clientWalletService = clientWalletService;
            _logger = logger;
        }


        public async ValueTask<List<ClientWallet>> GetWalletsAsync(JetClientIdentity clientId)
        {
            var list = await _clientWalletService.GetWalletsByClient(clientId);
            return list.Wallets;
        }

        public async ValueTask<ClientWallet> GetDefaultWalletAsync(JetClientIdentity clientId)
        {
            var list = await _clientWalletService.GetWalletsByClient(clientId);

            var defaultWallet = list.Wallets.FirstOrDefault(e => e.IsDefault) ?? list.Wallets.FirstOrDefault();

            if (defaultWallet == null)
            {
                _logger.LogError("Cannot found default wallet for Broker/Brand/Client: {brokerId}/{brandId}/{clientId}", clientId.BrokerId, clientId.BrandId, clientId.ClientId);
                throw new Exception($"Cannot found default wallet for Broker/Brand/Client: {clientId.BrokerId}/{clientId.BrandId}/{clientId.ClientId}");
            }

            return defaultWallet;
        }

        public async ValueTask<ClientWallet> GetWalletByIdAsync(JetClientIdentity clientId, string walletId)
        {
            var list = await _clientWalletService.GetWalletsByClient(clientId);

            var wallet = list.Wallets.FirstOrDefault(e => e.WalletId == walletId);

            return wallet;
        }

        public async ValueTask<JetWalletIdentity> GetWalletIdentityByIdAsync(JetClientIdentity clientId, string walletId)
        {
            var wallet = await GetWalletByIdAsync(clientId, walletId);

            if (wallet == null)
                return null;

            return new JetWalletIdentity(clientId.BrokerId, clientId.BrandId, clientId.ClientId, wallet.WalletId);
        }
    }
}