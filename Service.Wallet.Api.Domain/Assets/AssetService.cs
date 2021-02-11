using System.Collections.Generic;
using System.Linq;
using MyJetWallet.Domain;
using Service.AssetsDictionary.Client;
using Service.Wallet.Api.Domain.Models.Assets;

namespace Service.Wallet.Api.Domain.Assets
{
    public class AssetService : IAssetService
    {
        private readonly IAssetsDictionaryClient _assetsDictionaryClient;
        private readonly ISpotInstrumentDictionaryClient _spotInstrumentDictionaryClient;

        public AssetService(
            IAssetsDictionaryClient assetsDictionaryClient,
            ISpotInstrumentDictionaryClient spotInstrumentDictionaryClient)
        {
            _assetsDictionaryClient = assetsDictionaryClient;
            _spotInstrumentDictionaryClient = spotInstrumentDictionaryClient;
        }

        public List<WalletAsset> GetWalletAssets(IJetWalletIdentity wallet)
        {
            var assets = _assetsDictionaryClient
                .GetAssetsByBrand(wallet)
                .Where(a => a.IsEnabled)
                .Select(WalletAsset.Create)
                .ToList();

            return assets;
        }

        public List<WalletSpotInstrument> GetWalletSpotInstrument(IJetWalletIdentity wallet)
        {
            var instruments = _spotInstrumentDictionaryClient
                .GetSpotInstrumentByBrand(wallet)
                .Where(e => e.IsEnabled)
                .Select(WalletSpotInstrument.Create)
                .ToList();

            return instruments;
        }
    }
}