using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Domain.Prices;
using Service.ActiveOrders.Grpc.Models;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
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

            await SendAsync(HubNames.WalletList, message);
        }

        public async Task SendWalletAssetsAsync()
        {
            var wallet = GetWalletId();

            var assets = _assetService.GetWalletAssets(wallet);

            var message = new AssetListMessage()
            {
                Assets = assets
            };

            await SendAsync(HubNames.AssetList, message);
        }

        public async Task SendWalletSpotInstrumentsAsync()
        {
            var wallet = GetWalletId();

            var instruments = _assetService.GetWalletSpotInstrument(wallet);

            var message = new SpotInstrumentListMessage()
            {
                SpotInstruments = instruments
            };

            await SendAsync(HubNames.SpotInstrumentList, message);
        }
        
        private JetClientIdentity GetClientId()
        {
            if (ClientId == null)
                throw new Exception($"SignalR connection ({ConnectionId}) doesn't inited");

            return ClientId;
        }

        private JetWalletIdentity GetWalletId()
        {
            if (WalletId == null)
                throw new Exception($"SignalR connection ({ConnectionId}) doesn't associate with wallet");

            return WalletId;
        }

        private Task SendAsync(string method, object message)
        {
            return ClientProxy.SendAsync(method, message);
        }

        public async Task SendPongAsync()
        {
            await SendAsync(HubNames.Pong, new PongMessage());
        }

        public async Task SendCurrentPrices()
        {
            var prices = _currentPricesCache.GetPrices(ClientId.BrokerId);

            await SendPrices(prices);
        }

        public async Task SendPrices(IEnumerable<BidAsk> prices)
        {
            var message = new BidAskMessage()
            {
                Prices = prices.Select(BidAskContract.Create)
            };

            await SendAsync(HubNames.BidAsk, message);
        }

        public async Task SendWalletBalancesAsync()
        {
            var data = await _balanceService.GetBalancesByWallet(WalletId.WalletId);

            var message = new WalletBalancesMessage()
            {
                Balances = data ?? new List<WalletBalance>()
            };

            await SendAsync(HubNames.WalletBalances, message);

        }

        public async Task SendActiveOrdersAsync()
        {
            var data = await _activeOrderService.GetActiveOrdersAsync(new GetActiveOrdersRequest()
            {
                WalletId = WalletId.WalletId
            });

            var message = new ActiveOrdersMessage()
            {
                Orders = data?.Orders ?? new List<SpotOrder>()
            };

            await SendAsync(HubNames.ActiveOrders, message);
        }
    }
}