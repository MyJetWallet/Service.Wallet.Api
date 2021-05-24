using System;
using MyJetWallet.Domain.Orders;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class ExecuteSwapQuoteResponse
    {
        public bool IsExecuted { get; set; }
        public string InstrumentSymbol { get; set; }
        public string AssetSymbol { get; set; }
        public OrderSide Side { get; set; }
        public double Volume { get; set; }
        public string OperationId { get; set; }
        public double Price { get; set; }
        public double OppositeVolume { get; set; }
        public string OppositeAssetSymbol { get; set; }
        public DateTime ExpireTime { get; set; }
    }
}