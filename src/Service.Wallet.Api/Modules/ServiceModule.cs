using Autofac;
using MyJetWallet.Sdk.Service;
using Service.Wallet.Api.Domain.Assets;
using Service.Wallet.Api.Domain.Wallets;
using Service.Wallet.Api.Settings;
using SimpleTrading.SettingsReader;

namespace Service.Wallet.Api.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<AssetService>()
                .As<IAssetService>()
                .SingleInstance();

            builder
                .RegisterType<WalletService>()
                .As<IWalletService>()
                .SingleInstance();
        }
    }
}