using System.Collections.Generic;
using Service.Wallet.Api.Domain.Models.Assets;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.MarketReference)]
    public class MarketReferenceListMessage : MesssageContract
    {
        public List<MarketReference> References { get; set; }
    }
}