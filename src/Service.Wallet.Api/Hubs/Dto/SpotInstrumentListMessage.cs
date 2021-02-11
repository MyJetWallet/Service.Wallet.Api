using System.Collections.Generic;
using Service.Wallet.Api.Domain.Models.Assets;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.SpotInstrumentList)]
    public class SpotInstrumentListMessage: MesssageContract
    {
        public List<WalletSpotInstrument> SpotInstruments { get; set; }
    }
}