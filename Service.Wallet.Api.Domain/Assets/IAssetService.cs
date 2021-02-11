using System.Collections.Generic;
using System.Diagnostics;
using MyJetWallet.Domain;
using Service.Wallet.Api.Domain.Models.Assets;

namespace Service.Wallet.Api.Domain.Assets
{
    public interface IAssetService
    {
        List<WalletAsset> GetWalletAssets(JetWalletIdentity wallet);
        List<WalletSpotInstrument> GetWalletSpotInstrument(JetWalletIdentity wallet);
    }
}