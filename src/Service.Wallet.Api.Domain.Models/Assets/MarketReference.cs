using Service.AssetsDictionary.Domain.Models;

namespace Service.Wallet.Api.Domain.Models.Assets
{
    public class MarketReference
    {
        public string Id { get; set; }
        public string BrokerId { get; set; }
        public string Name { get; set; }
        public string IconUrl { get; set; }
        public string AssociateAsset { get; set; }
        public string AssociateAssetPair { get; set; }
        public int Weight { get; set; }

        public static MarketReference Create(IMarketReference reference)
        {
            return new MarketReference()
            {
                Id = reference.Id,
                AssociateAsset = reference.AssociateAsset,
                AssociateAssetPair = reference.AssociateAssetPair,
                BrokerId = reference.BrokerId,
                IconUrl = reference.IconUrl,
                Name = reference.Name,
                Weight = reference.Weight
            };
        }
    }
}