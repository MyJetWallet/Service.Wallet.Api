using MyJetWallet.Domain.Orders;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class GetSwapQuoteRequest : WalletRequest
    {
        public string FromAsset { get; set; }

        public string ToAsset { get; set; }

        public decimal? FromAssetVolume { get; set; }
        
        public decimal? ToAssetVolume { get; set; }

        /// <summary>
        /// true = from filled
        /// false = to filled
        /// </summary>
        public bool IsFromFixed { get; set; }
    }
}