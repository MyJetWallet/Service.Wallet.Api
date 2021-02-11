using System.Collections.Generic;
using Service.Wallet.Api.Domain.Models.Assets;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.AssetList)]
    public class AssetListMessage: MesssageContract
    {
        public List<WalletAsset> Assets { get; set; }
    }
}