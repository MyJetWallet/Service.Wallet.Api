using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.AssetsDictionary.Client;
using Service.Wallet.Api.Domain.Assets;
using Service.Wallet.Api.Hubs;

namespace Service.Wallet.Api.Jobs
{
    public class AssetDictionaryChangesNotificator: IStartable
    {
        private readonly IHubManager _hubManager;
        private readonly ILogger<AssetDictionaryChangesNotificator> _logger;
        private MyTaskTimer _timer;
        private bool _isAssetChanged = false;
        private bool _isSpotInstrumentChanged = false;

        public AssetDictionaryChangesNotificator(
            IHubManager hubManager,
            IAssetService assetService,
            ILogger<AssetDictionaryChangesNotificator> logger)
        {
            _hubManager = hubManager;
            _logger = logger;
            assetService.SubscribeToChanges(Changed);
        }

        private void Changed()
        {
            _isAssetChanged = true;
            _isSpotInstrumentChanged = true;
        }

        public void Start()
        {
            _timer = new MyTaskTimer(nameof(AssetDictionaryChangesNotificator),TimeSpan.FromSeconds(1), _logger, DoTime)
            {
                IsTelemetryActive = false
            };
            _timer.Start();
        }

        private async Task DoTime()
        {
            if (_isAssetChanged)
            {
                _isAssetChanged = false;

                await _hubManager.ExecForeachConnection(async connection => { await connection.SendWalletAssetsAsync(); });
            }

            if (_isSpotInstrumentChanged)
            {
                _isSpotInstrumentChanged = false;

                await _hubManager.ExecForeachConnection(async connection => { await connection.SendWalletSpotInstrumentsAsync(); });
            }
        }
    }
}