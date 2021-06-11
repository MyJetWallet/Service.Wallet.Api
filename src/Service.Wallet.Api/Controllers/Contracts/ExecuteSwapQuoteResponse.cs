using System;
using MyJetWallet.Domain.Orders;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class ExecuteSwapQuoteResponse
    {
        public bool IsExecuted { get; set; }

        public string OperationId { get; set; }
        public decimal Price { get; set; }

        public string FromAsset { get; set; }
        public string ToAsset { get; set; }
        public decimal FromAssetVolume { get; set; }
        public decimal ToAssetVolume { get; set; }

        public bool IsFromFixed { get; set; }

        public int ActualTimeInSecond { get; set; }
    }
}