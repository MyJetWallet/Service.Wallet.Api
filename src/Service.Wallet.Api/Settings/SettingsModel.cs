using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Wallet.Api.Settings
{
    public class SettingsModel
    {
        [YamlProperty("WalletApi.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("WalletApi.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
        [YamlProperty("WalletApi.AuthMyNoSqlReaderHostPort")]
        public string AuthMyNoSqlReaderHostPort { get; set; }

        [YamlProperty("WalletApi.ClientWalletsGrpcServiceUrl")]
        public string ClientWalletsGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("WalletApi.RegistrationGrpcServiceUrl")]
        public string RegistrationGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.MatchingEngineApiGrpcServiceUrl")]
        public string MatchingEngineApiGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.BalancesGrpcServiceUrl")]
        public string BalancesGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.ActiveOrdersGrpcServiceUrl")]
        public string ActiveOrdersGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.TradeHistoryGrpcServiceUrl")]
        public string TradeHistoryGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.BitgoDepositDetectorGrpcServiceUrl")]
        public string BitgoDepositDetectorGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.KycGrpcServiceUrl")]
        public string KycGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.BitgoCryptoWithdrawalGrpcServiceUrl")]
        public string BitgoCryptoWithdrawalGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.BalanceHistoryGrpcServiceUrl")]
        public string BalanceHistoryGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.LiquidityConverterGrpcServiceUrl")]
        public string LiquidityConverterGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.ZipkinUrl")] public string ZipkinUrl { get; set; }

        [YamlProperty("WalletApi.ElkLogs")] public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("WalletApi.BaseCurrencyConverterGrpcServiceUrl")]
        public string BaseCurrencyConverterGrpcServiceUrl { get; set; }
    }
}