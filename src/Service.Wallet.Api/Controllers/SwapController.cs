#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Orders;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;
using Service.Authorization.Client.Http;
using Service.Liquidity.Converter.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Contracts;
using Service.Wallet.Api.Domain.Swaps;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/swap")]
    public class SwapController : ControllerBase
    {
        private readonly ISwapService _swapService;
        private readonly ISpotInstrumentDictionaryClient _instrumentDictionary;

        public SwapController(ISwapService swapService, ISpotInstrumentDictionaryClient instrumentDictionary)
        {
            _swapService = swapService;
            _instrumentDictionary = instrumentDictionary;
        }

        /// <summary>
        /// Create limit order on the wallet
        /// Errors:
        ///  * NoqEnoughLiquidityForConvert
        /// </summary>
        [HttpPost("get-quote")]
        public async Task<Response<GetSwapQuoteResponse>> GetSwapQuoteAsync([FromBody] GetSwapQuoteRequest request)
        {
            if (request == null) throw new WalletApiBadRequestException("request cannot be null");
            
            if ((!request.FromAssetVolume.HasValue && !request.ToAssetVolume.HasValue) ||
                (request.FromAssetVolume.HasValue && request.ToAssetVolume.HasValue))
            {
                throw new WalletApiBadRequestException("should be setup one of FromAssetVolume or ToAssetVolume");
            }
            
            if (request.FromAssetVolume.HasValue && request.FromAssetVolume.Value <= 0) throw new WalletApiBadRequestException("volume (from) cannot be zero or negative");
            if (request.ToAssetVolume.HasValue && request.ToAssetVolume.Value <= 0) throw new WalletApiBadRequestException("volume (to) cannot be zero or negative");
            
            if (string.IsNullOrEmpty(request.FromAsset))
                throw new WalletApiBadRequestException("FromAsset cannot be empty");

            if (string.IsNullOrEmpty(request.ToAsset))
                throw new WalletApiBadRequestException("ToAsset cannot be empty");

            var walletId = this.GetWalletIdentity();
            
            var instrument = SelectInstrument(request.FromAsset, request.ToAsset, walletId);

            if (instrument == null)
            {
                throw new WalletApiErrorException("Cannot find way to convert assets",  ApiResponseCodes.NoqEnoughLiquidityForConvert);
            }

            Quote quote = null;

            if (request.FromAssetVolume.HasValue)
            {
                quote = await _swapService.GetSwapQuoteAsync(walletId, instrument.Symbol,
                    request.FromAsset, request.FromAssetVolume.Value, OrderSide.Sell);

                var response = new GetSwapQuoteResponse()
                {
                    FromAsset = request.FromAsset,
                    ToAsset = request.ToAsset,

                    OperationId = quote.OperationId,
                    Price = quote.Price,

                    ActualTimeInSecond = (int)(quote.ExpireTime - DateTime.UtcNow).TotalSeconds,

                    FromAssetVolume = quote.Volume,
                    ToAssetVolume = quote.OppositeVolume
                };

                return new Response<GetSwapQuoteResponse>(response);
            }
            else
            {
#pragma warning disable 8629
                quote = await _swapService.GetSwapQuoteAsync(walletId, instrument.Symbol, request.FromAsset, request.ToAssetVolume.Value, OrderSide.Buy);
#pragma warning restore 8629

                var response = new GetSwapQuoteResponse()
                {
                    FromAsset = request.FromAsset,
                    ToAsset = request.ToAsset,

                    OperationId = quote.OperationId,
                    Price = quote.Price,

                    ActualTimeInSecond = (int)(quote.ExpireTime - DateTime.UtcNow).TotalSeconds,

                    FromAssetVolume = quote.OppositeVolume,
                    ToAssetVolume = quote.Volume
                };

                return new Response<GetSwapQuoteResponse>(response);
            }

            throw new WalletApiErrorException("Cannot process quote request", ApiResponseCodes.NoqEnoughLiquidityForConvert);
        }

        /// <summary>
        /// Create limit order on the wallet
        /// </summary>
        [HttpPost("execute-quote")]
        public async Task<Response<ExecuteSwapQuoteResponse>> ExecuteSwapQuoteAsync(
            [FromBody] ExecuteSwapQuoteRequest request)
        {
            if (request == null) throw new WalletApiBadRequestException("request cannot be null");
            if (request.FromAssetVolume <= 0) throw new WalletApiBadRequestException("volume cannot be zero or negative");
            if (request.ToAssetVolume <= 0) throw new WalletApiBadRequestException("volume cannot be zero or negative");
            if (request.Price <= 0) throw new WalletApiBadRequestException("price cannot be zero or negative");
            if (string.IsNullOrEmpty(request.FromAsset))
                throw new WalletApiBadRequestException("FromAsset cannot be empty");
            if (string.IsNullOrEmpty(request.ToAsset))
                throw new WalletApiBadRequestException("ToAsset cannot be empty");
            if (string.IsNullOrEmpty(request.OperationId))
                throw new WalletApiBadRequestException("OperationId cannot be empty");
            
            var walletId = this.GetWalletIdentity();

            //todo: прокинуть параметры в конвертор чтоб там их даблчекнуть
            var (executed, quote) = await _swapService.ExecuteSwapQuoteAsync(walletId, request.OperationId);

            var response = new ExecuteSwapQuoteResponse()
            {
                IsExecuted = executed,

                //todo: заполнить респонс квотой
                
            };

            return new Response<ExecuteSwapQuoteResponse>(response);
        }


        private ISpotInstrument? SelectInstrument(string fromAsset, string toAsset, IJetBrandIdentity brand)
        {
            var instruments = _instrumentDictionary.GetSpotInstrumentByBrand(brand);

            ISpotInstrument? instrument = instruments.FirstOrDefault(e =>
                e.BaseAsset == fromAsset && e.QuoteAsset == toAsset ||
                e.BaseAsset == toAsset && e.QuoteAsset == fromAsset);

            return instrument;
        }
    }
}