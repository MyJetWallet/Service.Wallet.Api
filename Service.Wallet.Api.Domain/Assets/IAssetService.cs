using System;
using System.Collections.Generic;
using System.Diagnostics;
using MyJetWallet.Domain;
using Newtonsoft.Json.Bson;
using Service.Wallet.Api.Domain.Models.Assets;

namespace Service.Wallet.Api.Domain.Assets
{
    public interface IAssetService
    {
        List<WalletAsset> GetWalletAssets(IJetWalletIdentity wallet);
        List<WalletSpotInstrument> GetWalletSpotInstrument(IJetWalletIdentity wallet);
        List<MarketReference> GetMarketReference(IJetWalletIdentity wallet);
        void SubscribeToChanges(Action callback);
    }
}