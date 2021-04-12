using System;
using DotNetCoreDecorators;
using MessagePack;
using MyJetWallet.Domain.Prices;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.BidAsk + " --> Price")]
    [MessagePackObject]
    public class BidAskContract
    {
        [Key("S")]
        public string Id { get; set; }

        [Key("T")]
        public long DateTime { get; set; }

        [Key("B")]
        public double Bid { get; set; }

        [Key("A")]
        public double Ask { get; set; }
        
        public static BidAskContract Create(BidAsk price)
        {
            return new BidAskContract()
            {
                Id = price.Id,
                DateTime = price.DateTime.UnixTime(),
                Ask = price.Ask,
                Bid = price.Bid
            };
        }
    }
}