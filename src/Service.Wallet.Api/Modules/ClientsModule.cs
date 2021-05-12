using Autofac;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using Service.ActiveOrders.Client;
using Service.AssetsDictionary.Client;
using Service.Authorization.Client;
using Service.BalanceHistory.Client;
using Service.Balances.Client;
using Service.Bitgo.DepositDetector.Client;
using Service.Bitgo.WithdrawalProcessor.Client;
using Service.ClientWallets.Client;
using Service.MatchingEngine.Api.Client;
using Service.MatchingEngine.PriceSource.Client;
using Service.Registration.Client;
using Service.Service.KYC.Client;
using Service.TradeHistory.Client;

namespace Service.Wallet.Api.Modules
{
    public class ClientsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = new MyNoSqlTcpClient(
                Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort),
                ApplicationEnvironment.HostName ?? $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            builder.RegisterInstance(myNoSqlClient).AsSelf().SingleInstance();



            builder.RegisterAssetsDictionaryClients(myNoSqlClient);

            builder.RegisterAssetPaymentSettingsClients(myNoSqlClient);

            builder.RegisterClientWalletsClients(myNoSqlClient, Program.Settings.ClientWalletsGrpcServiceUrl);

            builder.RegisterMatchingEnginePriceSourceClient(myNoSqlClient);

            builder.RegisterClientRegistrationClient(myNoSqlClient, Program.Settings.RegistrationGrpcServiceUrl);

            builder.RegisterMatchingEngineApiClient(Program.Settings.MatchingEngineApiGrpcServiceUrl);

            builder.RegisterBalancesClients(Program.Settings.BalancesGrpcServiceUrl, myNoSqlClient);

            builder.RegisterActiveOrdersClients(Program.Settings.ActiveOrdersGrpcServiceUrl, myNoSqlClient);

            builder.RegisterTradeHistoryClient(Program.Settings.TradeHistoryGrpcServiceUrl);

            builder.RegisterBitgoDepositAddressClient(Program.Settings.BitgoDepositDetectorGrpcServiceUrl,
                myNoSqlClient);

            builder.RegisterBitgoCryptoWithdrawalClient(Program.Settings.BitgoCryptoWithdrawalGrpcServiceUrl);

            builder.RegisterKycStatusClients(myNoSqlClient, Program.Settings.KycGrpcServiceUrl);

            builder.RegisterBalanceHistoryClient(Program.Settings.BalanceHistoryGrpcServiceUrl);

            builder.RegisterAuthorizationClient(Program.Settings.AuthorizationGrpcServiceUrl);
            builder.RegisterAuthorizationSessionCache(myNoSqlClient);
        }
    }
}