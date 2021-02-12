using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Service.AssetsDictionary.Client;
using Service.Wallet.Api.Hubs;

namespace Service.Wallet.Api.Jobs
{
    public class AssetDictionaryChangesNotificator: IStartable
    {
        private readonly IHubManager _hubManager;
        private Timer _timer;
        private bool _isAssetChanged = false;
        private bool _isSpotInstrumentChanged = false;

        public AssetDictionaryChangesNotificator(
            IHubManager hubManager,
            IAssetsDictionaryClient assetsDictionaryClient,
            ISpotInstrumentDictionaryClient spotInstrumentDictionaryClient)
        {
            _hubManager = hubManager;
            assetsDictionaryClient.OnChanged += AssetChanged;
            spotInstrumentDictionaryClient.OnChanged += SpotInstrumentChanged;
        }

        private void AssetChanged()
        {
            _hubManager.ExecForeachConnection(async connection => { await connection.SendWalletAssetsAsync(); }).GetAwaiter().GetResult();
            //_isAssetChanged = true;
        }

        private void SpotInstrumentChanged()
        {
            _hubManager.ExecForeachConnection(async connection => { await connection.SendWalletSpotInstrumentsAsync(); }).GetAwaiter().GetResult();
            //_isSpotInstrumentChanged = true;
        }

        public void Start()
        {
            _timer = new Timer(DoTime, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void DoTime(object state)
        {
            if (_isAssetChanged)
            {
                _isAssetChanged = false;

                _hubManager.ExecForeachConnection(async connection => { await connection.SendWalletAssetsAsync(); }).GetAwaiter().GetResult();
            }

            if (_isSpotInstrumentChanged)
            {
                _isSpotInstrumentChanged = false;

                _hubManager.ExecForeachConnection(async connection => { await connection.SendWalletSpotInstrumentsAsync(); }).GetAwaiter().GetResult();
            }
        }
    }
}