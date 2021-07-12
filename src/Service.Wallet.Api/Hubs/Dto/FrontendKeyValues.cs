using System.Collections.Generic;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.KeyValues)]
    public class FrontendKeyValues : MesssageContract
    {
        public List<FrontendKeyValue.Domain.Models.FrontKeyValue> Keys { get; set; }
    }
}