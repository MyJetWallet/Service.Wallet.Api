using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MyJetWallet.Domain;
using Service.Wallet.Api.Hubs.Dto;

namespace Service.Wallet.Api.Hubs
{
    public partial class HubClientConnection
    {
        public async Task SendWalletListAsync()
        {
            var clientId = GetClientId();

            var wallets = await _walletService.GetWalletsAsync(clientId);

            var message = new WalletListMessage {Wallets = wallets};

            await ClientProxy.SendAsync(HubNames.WalletList, message);
        }

        public async Task SendWalletAssetsAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task SendWalletSpotInstrumentsAsync()
        {
            throw new System.NotImplementedException();
        }

        private IJetClientIdentity GetClientId()
        {
            if (ClientId == null)
                throw new Exception($"SignalR connection ({ConnectionId}) doesn't inited");

            return ClientId;
        }
    }
}