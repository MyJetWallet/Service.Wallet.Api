﻿using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Wallet.Api.Settings
{
    public class SettingsModel
    {
        [YamlProperty("WalletApi.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("WalletApi.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("WalletApi.ClientWalletsGrpcServiceUrl")]
        public string ClientWalletsGrpcServiceUrl { get; set; }
        
        [YamlProperty("WalletApi.BitgoDepositDetectorGrpcServiceUrl")]
        public string BitgoDepositDetectorGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.KycGrpcServiceUrl")]
        public string KycGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.BitgoCryptoWithdrawalGrpcServiceUrl")]
        public string BitgoCryptoWithdrawalGrpcServiceUrl { get; set; }
        
        [YamlProperty("WalletApi.PushNotificationGrpcServiceUrl")]
        public string PushNotificationGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.ZipkinUrl")] 
        public string ZipkinUrl { get; set; }

        [YamlProperty("WalletApi.ElkLogs")] 
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("WalletApi.BaseCurrencyConverterGrpcServiceUrl")]
        public string BaseCurrencyConverterGrpcServiceUrl { get; set; }

        [YamlProperty("WalletApi.EnableApiTrace")]
        public bool EnableApiTrace { get; set; }

        [YamlProperty("WalletApi.FrontendKeyValueGrpcServiceUrl")]
        public string FrontendKeyValueGrpcServiceUrl { get; set; }
    }
}