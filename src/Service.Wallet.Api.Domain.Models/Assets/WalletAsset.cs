using System.Diagnostics;
using Service.AssetsDictionary.Domain.Models;

namespace Service.Wallet.Api.Domain.Models.Assets
{
    public class WalletAsset
    {
        public string Symbol { get; set; }
        public string Description { get; set; }
        public int Accuracy { get; set; }
        public AvailabilityMode DepositMode { get; set; }
        public AvailabilityMode WithdrawalMode { get; set; }
        public TagType TagType { get; set; }

        public static WalletAsset Create(IAsset asset, bool cryptoDepositEnable, bool cryptoWithdrawalEnable)
        {
            
            return new WalletAsset()
            {
                Symbol = asset.Symbol,
                Accuracy = asset.Accuracy,
                Description = asset.Description,
                DepositMode = cryptoDepositEnable ? AvailabilityMode.Enabled : AvailabilityMode.Disabled,
                WithdrawalMode = cryptoWithdrawalEnable ? AvailabilityMode.Enabled : AvailabilityMode.Disabled,
                TagType = GetTagType(asset.Symbol),
            };
        }

        private static TagType GetTagType(string asset)
        {
            return asset switch
            {
                "XRP" => TagType.Tag,
                "XLM" => TagType.Memo,
                _ => TagType.None
            };
        }
    }
}