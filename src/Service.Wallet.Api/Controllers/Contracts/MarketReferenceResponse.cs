namespace Service.Wallet.Api.Controllers.Contracts
{
    public class MarketReferenceResponse
    {
        public string Id { get; set; }
        public string BrokerId { get; set; }
        public string Name { get; set; }
        public string IconUrl { get; set; }
        public string AssociateAsset { get; set; }
        public string AssociateAssetPair { get; set; }
        public int Weight { get; set; }
    }
}