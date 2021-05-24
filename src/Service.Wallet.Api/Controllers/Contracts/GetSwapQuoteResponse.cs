using System;
using MyJetWallet.Domain.Orders;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class GetSwapQuoteResponse
    {
        public string OperationId { get; set; }
        public double Price { get; set; }
        
        public string FromAsset { get; set; }
        public string ToAsset { get; set; }
        public double FromAssetVolume { get; set; }
        public double ToAssetVolume { get; set; }
        public bool IsFromFixed { get; set; }

        public int ActualTimeInSecond { get; set; }

    }
}