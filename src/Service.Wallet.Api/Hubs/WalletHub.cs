using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using Newtonsoft.Json;
using Service.ActiveOrders.Grpc;
using Service.Authorization.Client.Http;
using Service.Balances.Grpc;
using Service.MatchingEngine.PriceSource.Client;
using Service.Registration.Grpc;
using Service.Registration.Grpc.Models;
using Service.Wallet.Api.Controllers;
using Service.Wallet.Api.Domain.Assets;
using Service.Wallet.Api.Domain.Wallets;
using Service.Wallet.Api.Hubs.Dto;
using SimpleTrading.TokensManager;

namespace Service.Wallet.Api.Hubs
{
    /// <summary>
    /// Signal-R hub to send changes
    /// </summary>
    public class WalletHub: Hub
    {
        private readonly ILogger<WalletHub> _logger;
        private readonly IHubManager _hubManager;
        private readonly IAssetService _assetService;
        private readonly IWalletService _walletService;
        private readonly ICurrentPricesCache _currentPricesCache;
        private readonly IClientRegistrationService _clientRegistrationService;
        private readonly IWalletBalanceService _balanceService;
        private readonly IActiveOrderService _orderService;


        public const string AccessTokenParamName = "access_token";

        public WalletHub(ILogger<WalletHub> logger,
            IHubManager hubManager,
            IAssetService assetService,
            IWalletService walletService,
            ICurrentPricesCache currentPricesCache,
            IClientRegistrationService clientRegistrationService,
            IWalletBalanceService balanceService,
            IActiveOrderService orderService)
        {
            _logger = logger;
            _hubManager = hubManager;
            _assetService = assetService;
            _walletService = walletService;
            _currentPricesCache = currentPricesCache;
            _clientRegistrationService = clientRegistrationService;
            _balanceService = balanceService;
            _orderService = orderService;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            _logger.LogInformation("HUB [WalletHub] is connected. ConnectionId: {ConnectionId}", httpContext.Connection.Id);

            if (httpContext.Request.Query.ContainsKey(AccessTokenParamName))
            {
                var token = httpContext.Request.Query[AccessTokenParamName];
                if (!string.IsNullOrEmpty(token))
                    await Init(token);
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var httpContext = Context.GetHttpContext();

            var ctx = this.TryGetConnection();

            if (ctx != null)
            {
                // todo: Add to trader log SignalR Disconnection Event

                _hubManager.Disconnected(Context.ConnectionId);
            }

            _logger.LogInformation("HUB [WalletHub] is disconnected. ConnectionId: {ConnectionId}. Exception: {exception}. Broker/Brand/Client/Wallet: {brokerId}/{brandId}/{clientId}", 
                httpContext?.Connection?.Id, exception?.ToString(), ctx?.ClientId?.BrokerId, ctx?.ClientId?.BrandId, ctx?.ClientId?.ClientId, ctx?.WalletId?.WalletId);

            return base.OnDisconnectedAsync(exception);
        }

        [SignalRIncomingRequest]
        public async Task Init(string tokenString)
        {
            // todo: Add to trader log SignalR Connection Event

            var (result, token) = MyControllerBaseHelper.ParseToken(tokenString);

            if (result != TokenParseResult.Ok || !token.IsValid())
            {
                _logger.LogWarning("[HUB] Cannot parse token. Result: {resultText}; Token: {JsonText}",
                    result.ToString(), token != null ? JsonConvert.SerializeObject(token) : "null");

                Context.Abort();

                return;
            }

            var clientId = new JetClientIdentity(token.BrokerId, token.BrandId, token.ClientId());
            
            var ctx = new HubClientConnection(Context, Clients.Caller, clientId, _assetService, _walletService, _currentPricesCache, _balanceService, _orderService, _logger);
            ctx.SetWalletId(new JetWalletIdentity(token.BrokerId, token.BrandId, token.ClientId(), token.WalletId));

            _hubManager.Connected(ctx);

            var message = WelcomeMessage.Create($"Hello {token.ClientId()}/{token.WalletId}");

            await Clients.Caller.SendAsync(HubNames.Welcome, message);

            _logger.LogInformation("[HUB] Init connection ({connectionId}) for Broker/Brand/Client/Wallet: {brokerId}/{brandId}/{clientId}/{walletId}", ctx.ConnectionId, ctx.ClientId.BrokerId, ctx.ClientId.BrandId, ctx.ClientId.ClientId, ctx.WalletId.WalletId);

            //todo: Send wallet list (wallet name, walletId, is default)

            await ctx.SendWalletListAsync();

            await ctx.SendWalletAssetsAsync();
            await ctx.SendWalletSpotInstrumentsAsync();
            await ctx.SendWalletBalancesAsync();
            await ctx.SendActiveOrdersAsync();

            await ctx.SendCurrentPrices();
        }

        [SignalRIncomingRequest]
        public async Task SetOrderBook(string symbol)
        {
            var ctx = TryGetConnection();

            if (ctx == null)
            {
                _logger.LogInformation("[HUB][ERROR] Receive SetWallet but connection do not inited");
                return;
            }

            ctx.SetOrderBook(symbol);

            await ctx.SendOrderBook();
        }

        [SignalRIncomingRequest]
        public async Task Ping()
        {
            var ctx = TryGetConnection();

            if (ctx == null)
            {
                _logger.LogInformation("[HUB][ERROR] Receive Ping but connection do not inited");
                return;
            }

            await ctx.SendPongAsync();
        }

        private HubClientConnection TryGetConnection()
        {
            return _hubManager.TryGetContext(Context.ConnectionId);
        }
    }
}