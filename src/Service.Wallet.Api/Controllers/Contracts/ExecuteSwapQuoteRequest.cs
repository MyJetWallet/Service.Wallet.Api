using System;
using MyJetWallet.Domain.Orders;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class ExecuteSwapQuoteRequest : WalletRequest
    {
        public string OperationId { get; set; }
        public decimal Price { get; set; }

        public string FromAsset { get; set; }
        public string ToAsset { get; set; }
        public decimal FromAssetVolume { get; set; }
        public decimal ToAssetVolume { get; set; }

        public bool IsFromFixed { get; set; }
    }
}