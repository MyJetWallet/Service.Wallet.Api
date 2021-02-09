using SimpleTrading.SettingsReader;

namespace Service.Wallet.Api.Settings
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("WalletApi.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }
    }
}