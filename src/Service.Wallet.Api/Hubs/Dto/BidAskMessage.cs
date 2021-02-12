using System.Collections.Generic;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.AssetList)]
    public class BidAskMessage : MesssageContract
    {
        public IEnumerable<BidAskContract> Prices { get; set; }
    }
}