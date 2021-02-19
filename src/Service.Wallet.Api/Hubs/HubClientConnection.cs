using Microsoft.AspNetCore.SignalR;
using MyJetWallet.Domain;
using Service.Balances.Grpc;
using Service.MatchingEngine.PriceSource.Client;
using Service.Wallet.Api.Controllers;
using Service.Wallet.Api.Domain.Assets;
using Service.Wallet.Api.Domain.Wallets;

namespace Service.Wallet.Api.Hubs
{
    public partial class HubClientConnection
    {
        private readonly HubCallerContext _context;
        private readonly IAssetService _assetService;
        private readonly IWalletService _walletService;
        private readonly ICurrentPricesCache _currentPricesCache;
        private readonly IWalletBalanceService _balanceService;

        public HubClientConnection(
            HubCallerContext context, 
            IClientProxy client,
            JetClientIdentity clientId,
            IAssetService assetService,
            IWalletService walletService,
            ICurrentPricesCache currentPricesCache,
            IWalletBalanceService balanceService)
        {
            _context = context;
            _assetService = assetService;
            _walletService = walletService;
            _currentPricesCache = currentPricesCache;
            _balanceService = balanceService;
            ClientProxy = client;

            var httpContext = _context.GetHttpContext();

            Ip = httpContext.GetIp();

            UserAgent = httpContext.GetUserAgent();

            ClientId = clientId;
        }

        public void SetWalletId(JetWalletIdentity walletId)
        {
            WalletId = walletId;
        }

        // client context for the connection
        //
        //public string ActiveAccountId { get; set; }
        //public string TraderId { get; }
        //public Dictionary<string, MtInstrumentHubModel> Instruments = new Dictionary<string, MtInstrumentHubModel>();
        //public Dictionary<string, MtInstrumentGroupHubModel> InstrumentGroups = new Dictionary<string, MtInstrumentGroupHubModel>();

        public string ConnectionId => _context.ConnectionId;

        public string Ip { get; }

        public string UserAgent { get; }

        public IClientProxy ClientProxy { get; }

        public JetClientIdentity ClientId { get; }

        public JetWalletIdentity WalletId { get; private set; }
    }
}