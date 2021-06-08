using Autofac;
using MyJetWallet.Sdk.Authorization.NoSql;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using Service.ActiveOrders.Client;
using Service.AssetsDictionary.Client;
using Service.BalanceHistory.Client;
using Service.Balances.Client;
using Service.BaseCurrencyConverter.Client;
using Service.Bitgo.DepositDetector.Client;
using Service.Bitgo.WithdrawalProcessor.Client;
using Service.ClientWallets.Client;
using Service.Liquidity.Converter.Client;
using Service.MatchingEngine.Api.Client;
using Service.MatchingEngine.PriceSource.Client;
using Service.PushNotification.Client;
using Service.Registration.Client;
using Service.Service.KYC.Client;

namespace Service.Wallet.Api.Modules
{
    public class ClientsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));

            builder.RegisterAssetsDictionaryClients(myNoSqlClient);

            builder.RegisterAssetPaymentSettingsClients(myNoSqlClient);

            builder.RegisterClientWalletsClients(myNoSqlClient, Program.Settings.ClientWalletsGrpcServiceUrl);

            builder.RegisterMatchingEnginePriceSourceClient(myNoSqlClient);

            builder.RegisterClientRegistrationClient(myNoSqlClient, Program.Settings.RegistrationGrpcServiceUrl);

            builder.RegisterMatchingEngineApiClient(Program.Settings.MatchingEngineApiGrpcServiceUrl);

            builder.RegisterBalancesClients(Program.Settings.BalancesGrpcServiceUrl, myNoSqlClient);

            builder.RegisterActiveOrdersClients(Program.Settings.ActiveOrdersGrpcServiceUrl, myNoSqlClient);

            builder.RegisterBitgoDepositAddressClient(Program.Settings.BitgoDepositDetectorGrpcServiceUrl, myNoSqlClient);

            builder.RegisterBitgoCryptoWithdrawalClient(Program.Settings.BitgoCryptoWithdrawalGrpcServiceUrl);

            builder.RegisterKycStatusClients(myNoSqlClient, Program.Settings.KycGrpcServiceUrl);

            builder.RegisterBalanceHistoryClient(Program.Settings.BalanceHistoryGrpcServiceUrl);
            builder.RegisterTradeHistoryClient(Program.Settings.BalanceHistoryGrpcServiceUrl);
            builder.RegisterSwapHistoryClient(Program.Settings.BalanceHistoryGrpcServiceUrl);
            
            builder.RegisterLiquidityConverterClient(Program.Settings.LiquidityConverterGrpcServiceUrl);

            builder.RegisterBaseCurrencyConverterClient(Program.Settings.BaseCurrencyConverterGrpcServiceUrl, myNoSqlClient);
            
            builder.RegisterPushNotificationClient(Program.Settings.PushNotificationGrpcServiceUrl);

            RegisterAuthServices(builder);
        }

        protected void RegisterAuthServices(ContainerBuilder builder)
        {
            // he we do not use CreateNoSqlClient beacuse we have a problem with start many mynosql instances 
            var authNoSql = new MyNoSqlTcpClient(
                Program.ReloadedSettings(e => e.AuthMyNoSqlReaderHostPort),
                ApplicationEnvironment.HostName ?? $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            builder.RegisterMyNoSqlReader<ShortRootSessionNoSqlEntity>(authNoSql, ShortRootSessionNoSqlEntity.TableName);

            authNoSql.Start();
        }
    }
}