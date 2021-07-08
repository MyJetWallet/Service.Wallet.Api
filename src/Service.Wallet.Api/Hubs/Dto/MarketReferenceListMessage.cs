using System.Collections.Generic;
using Service.Wallet.Api.Controllers.Contracts;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.MarketReference)]
    public class MarketReferenceListMessage : MesssageContract
    {
        public List<MarketReferenceResponse> References { get; set; }
    }
}