using Service.AssetsDictionary.Domain.Models;

namespace Service.Wallet.Api.Domain.Models.Assets
{
    public class WalletAsset
    {
        public string Symbol { get; set; }
        public string Description { get; set; }
        public int Accuracy { get; set; }

        public static WalletAsset Create(IAsset asset)
        {
            return new WalletAsset()
            {
                Symbol = asset.Symbol,
                Accuracy = asset.Accuracy,
                Description = asset.Description
            };
        }
    }
}