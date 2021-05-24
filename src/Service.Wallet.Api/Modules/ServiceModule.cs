﻿using Autofac;
using Service.Wallet.Api.Domain.Assets;
using Service.Wallet.Api.Domain.Orders;
using Service.Wallet.Api.Domain.Swaps;
using Service.Wallet.Api.Domain.Wallets;
using Service.Wallet.Api.Hubs;
using Service.Wallet.Api.Jobs;
using Service.Wallet.Api.Services;

namespace Service.Wallet.Api.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HubManager>()
                .As<IHubManager>()
                .SingleInstance();

            builder
                .RegisterType<AssetService>()
                .As<IAssetService>()
                .SingleInstance();

            builder
                .RegisterType<WalletService>()
                .As<IWalletService>()
                .SingleInstance();

            builder
                .RegisterType<AssetDictionaryChangesNotificator>()
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();

            builder
                .RegisterType<PriceChangesNotificator>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<OrderService>()
                .As<IOrderService>()
                .SingleInstance();

            builder
                .RegisterType<SwapService>()
                .As<ISwapService>()
                .SingleInstance();

            builder
                .RegisterType<BalancesNotificator>()
                .AsSelf()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<ActiveOrderNotificator>()
                .AsSelf()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<TradeNotificator>()
                .AsSelf()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<BlockchainIntegrationService>()
                .As<IBlockchainIntegrationService>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}