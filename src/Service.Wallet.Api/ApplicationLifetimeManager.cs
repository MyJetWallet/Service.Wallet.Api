using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.WalletApi;
using MyJetWallet.Sdk.WalletApi.Wallets;

namespace Service.Wallet.Api
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyNoSqlClientLifeTime _myNoSqlClientLifeTime;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, ILogger<ApplicationLifetimeManager> logger,
            IWalletService walletService,
            MyNoSqlClientLifeTime myNoSqlClientLifeTime)
            : base(appLifetime)
        {
            _logger = logger;
            _myNoSqlClientLifeTime = myNoSqlClientLifeTime;

            // init static locator to implement helper - Get wallet
            ControllerUtils.WalletService = walletService;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _myNoSqlClientLifeTime.Start();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            _myNoSqlClientLifeTime.Stop();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
