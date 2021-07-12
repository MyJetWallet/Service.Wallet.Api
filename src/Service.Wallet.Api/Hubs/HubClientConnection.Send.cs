using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Domain.Prices;
using Service.ActiveOrders.Grpc.Models;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
using Service.FrontendKeyValue.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;
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

            //_logger.LogInformation("[HUB] Send asset list. WalletId: {walletId}; Data: {jsonText}", WalletId, JsonConvert.SerializeObject(message));
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
        
        public async Task SendMarketReferenceAsync()
        {
            var references = new List<MarketReferenceResponse>()
            {
                new MarketReferenceResponse()
                {
                    Id = "BTC",
                    Name = "Bitcoin",
                    AssociateAsset = "BTC",
                    AssociateAssetPair = "BTCUSD",
                    BrokerId = "myjetwallet",
                    IconUrl = "",
                    Weight = 100
                },
                new MarketReferenceResponse()
                {
                    Id = "ETH",
                    Name = "Ethereum",
                    AssociateAsset = "ETH",
                    AssociateAssetPair = "ETHUSD",
                    BrokerId = "myjetwallet",
                    IconUrl = "",
                    Weight = 100
                },
            };
            var message = new MarketReferenceListMessage()
            {
                References = references
            };

            await SendAsync(HubNames.MarketReference, message);
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

        private async Task SendAsync(string method, object message)
        {
            try
            {
                await ClientProxy.SendAsync(method, message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Cannot send {method} to wallet '{walletId}' [{brokerId}|{brandId}|{clientId}] via connection {connectionId}",
                    method, WalletId.WalletId, WalletId.BrokerId, WalletId.BrandId, WalletId.ClientId, ConnectionId);
            }
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

        public Task SendTradesAsync(TradesMessage message)
        {
            return SendAsync(HubNames.Trades, message);
        }

        public async Task SendOrderBook()
        {
            

        }

        public async Task SendKeyValuesAsync()
        {
            var data = await _frontKeyValueService.GetKeysAsync(
                new GetFrontKeysRequest() {ClientId = WalletId.ClientId});

            var message = new FrontendKeyValues()
            {
                Keys = data.KeyValues
            };

            await SendAsync(HubNames.KeyValues, message);
        }
    }
}