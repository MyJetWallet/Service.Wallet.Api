using SimpleTrading.SettingsReader;

namespace Service.Wallet.Api.Settings
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("WalletApi.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("WalletApi.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("WalletApi.ClientWalletsGrpcServiceUrl")]
        public string ClientWalletsGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("WalletApi.RegistrationGrpcServiceUrl")]
        public string RegistrationGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.MatchingEngine.TradingGrpcServiceUrl")]
        public string MatchingEngineTradingGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.BalancesGrpcServiceUrl")]
        public string BalancesGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.ActiveOrdersGrpcServiceUrl")]
        public string ActiveOrdersGrpcServiceUrl { get; set; }
    }
}