using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Service.Wallet.Api.Domain.Assets;
using Service.Wallet.Api.Domain.Wallets;
using Service.Wallet.Api.Hubs.Dto;

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

        
        
        public const string AccessTokenParamName = "access_token";

        public WalletHub(ILogger<WalletHub> logger,
            IHubManager hubManager,
            IAssetService assetService,
            IWalletService walletService)
        {
            _logger = logger;
            _hubManager = hubManager;
            _assetService = assetService;
            _walletService = walletService;
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
        public async Task Init(string token)
        {
            Console.WriteLine($"--> [Init] {token}");

            // todo: Add to trader log SignalR Connection Event

            var ctx = new HubClientConnection(Context, Clients.Caller, token, _assetService, _walletService);

            _hubManager.Connected(ctx);

            var message = WelcomeMessage.Create($"Hello {token}");

            await Clients.Caller.SendAsync(HubNames.Welcome, message);

            _logger.LogInformation("[HUB] Init connection ({connectionId}) for Broker/Brand/Client: {brokerId}/{brandId}/{clientId}", ctx.ConnectionId, ctx.ClientId.BrokerId, ctx.ClientId.BrandId, ctx.ClientId.ClientId);

            //todo: Send wallet list (wallet name, walletId, is default)

            await ctx.SendWalletListAsync();

            var defaultWallet = await _walletService.GetDefaultWalletAsync(ctx.ClientId);

            await SetWallet(defaultWallet.WalletId);
        }

        [SignalRIncomingRequest]
        public async Task SetWallet(string walletId)
        {
            //todo: Send asset dictionary: [spot instrument list]; [send asset list (assetId, Name, accuracy,..., list of exchange assets and pairs detail)]
            //todo: associate connection with wallet
            //todo: send balances
            //todo: send active orders
            //todo: send asset deposit\withdrawal list (asset, deposit processors[], withdrawal processors[])

            var ctx = TryGetConnection();

            if (ctx == null)
            {
                _logger.LogInformation("[HUB][ERROR] Receive SetWallet but connection do not inited");
                return;
            }

            var wallet = await _walletService.GetWalletIdentityByIdAsync(ctx.ClientId, walletId);

            if (wallet == null)
            {
                _logger.LogInformation("[HUB][ERROR] Receive SetWallet with wrong walletId ({walletId}). Broker/Brand/Client: {brokerId}/{brandId}/{clientId}", walletId, ctx.ClientId.BrokerId, ctx.ClientId.BrandId, ctx.ClientId.ClientId);
            }

            ctx.SetWalletId(wallet);

            _logger.LogInformation("[HUB] Set wallet ({walletId}) to connection ({connectionId}) for Broker/Brand/Client: {brokerId}/{brandId}/{clientId}", 
                walletId, ctx.ConnectionId, ctx.ClientId.BrokerId, ctx.ClientId.BrandId, ctx.ClientId.ClientId);

            await ctx.SendWalletAssetsAsync();
            await ctx.SendWalletSpotInstrumentsAsync();
        }

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