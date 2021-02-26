﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.Registration.Grpc;
using Service.Wallet.Api.Jobs;

namespace Service.Wallet.Api
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly ActiveOrderNotificator _activeOrderNotificator;
        private readonly BalancesNotificator _balancesNotificator;
        private readonly TradeNotificator _tradeNotificator;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, ILogger<ApplicationLifetimeManager> logger,
            ActiveOrderNotificator activeOrderNotificator,
            BalancesNotificator balancesNotificator,
            TradeNotificator tradeNotificator)
            : base(appLifetime)
        {
            _logger = logger;
            _activeOrderNotificator = activeOrderNotificator;
            _balancesNotificator = balancesNotificator;
            _tradeNotificator = tradeNotificator;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _activeOrderNotificator.Start();
            _balancesNotificator.Start();
            _tradeNotificator.Start();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            _activeOrderNotificator.Stop();
            _balancesNotificator.Stop();
            _tradeNotificator.Stop();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
