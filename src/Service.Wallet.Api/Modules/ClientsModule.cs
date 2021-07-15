using Autofac;
using MyJetWallet.Sdk.Authorization.NoSql;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.Client;
using Service.BaseCurrencyConverter.Client;
using Service.Bitgo.DepositDetector.Client;
using Service.Bitgo.WithdrawalProcessor.Client;
using Service.ClientWallets.Client;
using Service.FrontendKeyValue.Client;
using Service.PushNotification.Client;
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
            
            
            builder.RegisterBitgoDepositAddressClient(Program.Settings.BitgoDepositDetectorGrpcServiceUrl, myNoSqlClient);

            builder.RegisterBitgoCryptoWithdrawalClient(Program.Settings.BitgoCryptoWithdrawalGrpcServiceUrl);

            builder.RegisterKycStatusClients(myNoSqlClient, Program.Settings.KycGrpcServiceUrl);

            
            builder.RegisterBaseCurrencyConverterClient(Program.Settings.BaseCurrencyConverterGrpcServiceUrl, myNoSqlClient);
            
            builder.RegisterPushNotificationClient(Program.Settings.PushNotificationGrpcServiceUrl);

            builder.RegisterFrontendKeyValueClient(myNoSqlClient, Program.Settings.FrontendKeyValueGrpcServiceUrl);

            RegisterAuthServices(builder);
        }

        protected void RegisterAuthServices(ContainerBuilder builder)
        {
            var authNoSql = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            builder.RegisterMyNoSqlReader<ShortRootSessionNoSqlEntity>(authNoSql, ShortRootSessionNoSqlEntity.TableName);
            authNoSql.Start();
        }
    }
}