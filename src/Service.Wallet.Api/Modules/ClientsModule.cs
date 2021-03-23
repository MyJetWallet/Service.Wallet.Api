using Autofac;
using MyJetWallet.MatchingEngine.Grpc;
using MyNoSqlServer.DataReader;
using Service.ActiveOrders.Client;
using Service.AssetsDictionary.Client;
using Service.BalanceHistory.Client;
using Service.Balances.Client;
using Service.Bitgo.DepositDetector.Client;
using Service.Bitgo.WithdrawalProcessor.Client;
using Service.ClientWallets.Client;
using Service.MatchingEngine.PriceSource.Client;
using Service.Registration.Client;
using Service.Service.KYC.Client;
using Service.TradeHistory.Client;

namespace Service.Wallet.Api.Modules
{
    public class ClientsModule: Module
    {
        private readonly MyNoSqlTcpClient _myNoSqlClient;

        public ClientsModule(MyNoSqlTcpClient myNoSqlClient)
        {
            _myNoSqlClient = myNoSqlClient;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssetsDictionaryClients(_myNoSqlClient);

            builder.RegisterAssetPaymentSettingsClients(_myNoSqlClient);

            builder.RegisterClientWalletsClients(_myNoSqlClient, Program.Settings.ClientWalletsGrpcServiceUrl);

            builder.RegisterMatchingEnginePriceSourceClient(_myNoSqlClient);

            builder.RegisterClientRegistrationClient(_myNoSqlClient, Program.Settings.RegistrationGrpcServiceUrl);

            builder.RegisterMatchingEngineGrpcClient(tradingServiceGrpcUrl: Program.Settings.MatchingEngineTradingGrpcServiceUrl);

            builder.RegisterBalancesClients(Program.Settings.BalancesGrpcServiceUrl, _myNoSqlClient);

            builder.RegisterActiveOrdersClients(Program.Settings.ActiveOrdersGrpcServiceUrl, _myNoSqlClient);

            builder.RegisterTradeHistoryClient(Program.Settings.TradeHistoryGrpcServiceUrl);

            builder.RegisterBitgoDepositAddressClient(Program.Settings.BitgoDepositDetectorGrpcServiceUrl, _myNoSqlClient);

            builder.RegisterBitgoCryptoWithdrawalClient(Program.Settings.BitgoCryptoWithdrawalGrpcServiceUrl);
            
            builder.RegisterKycStatusClients(_myNoSqlClient, Program.Settings.KycGrpcServiceUrl);

            builder.RegisterBalanceHistoryClient(Program.Settings.BalanceHistoryGrpcServiceUrl);
            
        }
    }
}