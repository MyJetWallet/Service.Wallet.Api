using System;
using MessagePack;
using MyJetWallet.Domain.Prices;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.AssetList)]
    [MessagePackObject]
    public class BidAskContract
    {
        [Key("S")]
        public string Id { get; set; }

        [Key("T")]
        public DateTime DateTime { get; set; }

        [Key("B")]
        public decimal Bid { get; set; }

        [Key("A")]
        public decimal Ask { get; set; }
        
        public static BidAskContract Create(BidAsk price)
        {
            return new BidAskContract()
            {
                Id = price.Id,
                DateTime = price.DateTime,
                Ask = price.Ask,
                Bid = price.Bid
            };
        }
    }
}