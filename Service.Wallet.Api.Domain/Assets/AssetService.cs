using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;
using Service.Wallet.Api.Domain.Models.Assets;

namespace Service.Wallet.Api.Domain.Assets
{
    public class AssetService : IAssetService
    {
        private readonly IAssetsDictionaryClient _assetsDictionaryClient;
        private readonly ISpotInstrumentDictionaryClient _spotInstrumentDictionaryClient;
        private readonly IAssetPaymentSettingsClient _paymentSettingsClient;
        private readonly ILogger<AssetService> _logger;

        private Dictionary<string, List<WalletAsset>> _assetCache = new Dictionary<string, List<WalletAsset>>();
        private Dictionary<string, List<WalletSpotInstrument>> _instrumentCache = new Dictionary<string, List<WalletSpotInstrument>>();
        private List<Action> _callbackList = new List<Action>();

        public AssetService(
            IAssetsDictionaryClient assetsDictionaryClient,
            ISpotInstrumentDictionaryClient spotInstrumentDictionaryClient,
            IAssetPaymentSettingsClient paymentSettingsClient,
            ILogger<AssetService> logger)
        {
            _assetsDictionaryClient = assetsDictionaryClient;
            _spotInstrumentDictionaryClient = spotInstrumentDictionaryClient;
            _paymentSettingsClient = paymentSettingsClient;
            _logger = logger;
            _assetsDictionaryClient.OnChanged += NoSqlDataChanged;
            _spotInstrumentDictionaryClient.OnChanged += NoSqlDataChanged;
            _paymentSettingsClient.OnChanged += NoSqlDataChanged;
        }

        private void NoSqlDataChanged()
        {
            lock (_instrumentCache)
            {
                _instrumentCache.Clear();
            }

            lock (_assetCache)
            {
                _assetCache.Clear();
            }

            RaiseChange();
        }

        public List<WalletAsset> GetWalletAssets(IJetWalletIdentity wallet)
        {
            var assets = GetGetWalletAssetsByBrand(wallet);
            return assets;
        }

        public List<WalletSpotInstrument> GetWalletSpotInstrument(IJetWalletIdentity wallet)
        {
            lock (_instrumentCache)
            {
                if (_instrumentCache.TryGetValue(wallet.BrandId, out var result))
                {
                    return result;
                }
            }
            
            var instruments = _spotInstrumentDictionaryClient
                .GetSpotInstrumentByBrand(wallet)
                .Where(e => e.IsEnabled)
                .Select(WalletSpotInstrument.Create)
                .ToList();
            
            lock (_instrumentCache)
            {
                _instrumentCache[wallet.BrandId] = instruments;
            }

            return instruments;
        }

        public void SubscribeToChanges(Action callback)
        {
            _callbackList.Add(callback);
        }

        private List<WalletAsset> GetGetWalletAssetsByBrand(IJetBrandIdentity brand)
        {
            lock (_assetCache)
            {
                if (_assetCache.TryGetValue(brand.BrandId, out var result))
                {
                    return result;
                }
            }

            var assets = _assetsDictionaryClient
                .GetAssetsByBrand(brand)
                .Where(a => a.IsEnabled);

            var paymentSettings = _paymentSettingsClient.GetAssetsByBroker(brand).ToDictionary(e => e.AssetSymbol);

            var list = new List<WalletAsset>();

            foreach (var asset in assets)
            {
                if (!paymentSettings.TryGetValue(asset.Symbol, out var payment))
                    payment = null;

                var item = WalletAsset.Create(asset,
                    payment?.BitGoCrypto?.IsEnabledDeposit == true,
                    payment?.BitGoCrypto?.IsEnabledWithdrawal == true);

                list.Add(item);
            }

            lock (_assetCache)
            {
                _assetCache[brand.BrandId] = list;
            }

            return list;
        }

        private void RaiseChange()
        {
            foreach (var action in _callbackList)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Cannot execute change callback");
                }
            }
        }

    }
}