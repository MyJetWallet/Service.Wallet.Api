﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyNoSqlServer.Abstractions;
using Service.ActiveOrders.Grpc;
using Service.Balances.Grpc;
using Service.BaseCurrencyConverter.Grpc;
using Service.FrontendKeyValue.Domain.Models.NoSql;
using Service.FrontendKeyValue.Grpc;
using Service.MatchingEngine.PriceSource.Client;
using Service.Wallet.Api.Controllers;
using Service.Wallet.Api.Domain.Assets;
using Service.Wallet.Api.Domain.Wallets;
using SimpleTrading.ClientApi.Utils;

namespace Service.Wallet.Api.Hubs
{
    public partial class HubClientConnection
    {
        private readonly HubCallerContext _context;
        private readonly IAssetService _assetService;
        private readonly IWalletService _walletService;
        private readonly ICurrentPricesCache _currentPricesCache;
        private readonly IWalletBalanceService _balanceService;
        private readonly IActiveOrderService _activeOrderService;
        private readonly IFrontKeyValueService _frontKeyValueService;
        private readonly ILogger _logger;

        public HubClientConnection(HubCallerContext context,
            IClientProxy client,
            JetClientIdentity clientId,
            IAssetService assetService,
            IWalletService walletService,
            ICurrentPricesCache currentPricesCache,
            IWalletBalanceService balanceService, 
            IActiveOrderService activeOrderService,
            IFrontKeyValueService frontKeyValueService,
            ILogger logger)
        {
            _context = context;
            _assetService = assetService;
            _walletService = walletService;
            _currentPricesCache = currentPricesCache;
            _balanceService = balanceService;
            _activeOrderService = activeOrderService;
            _frontKeyValueService = frontKeyValueService;
            _logger = logger;
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

        public void SetOrderBook(string symbol)
        {
            ActiveOrderBook = symbol;
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

        public string ActiveOrderBook { get; private set; }
    }
}