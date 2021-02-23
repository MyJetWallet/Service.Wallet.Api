using System.Collections.Generic;
using MyJetWallet.Domain.Orders;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.ActiveOrders)]
    public class ActiveOrdersMessage
    {
        public List<SpotOrder> Orders { get; set; }
    }
}