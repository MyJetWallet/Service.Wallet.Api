using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Authorization.Client.Http;
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

        public SwapController(ISwapService swapService)
        {
            _swapService = swapService;
        }

        /// <summary>
        /// Create limit order on the wallet
        /// </summary>
        [HttpPost("get-quote")]
        public async Task<Response<GetSwapQuoteResponse>> GetSwapQuoteAsync([FromBody] GetSwapQuoteRequest request)
        {
            if (request == null) throw new WalletApiBadRequestException("request cannot be null");
            if (request.Volume <= 0) throw new WalletApiBadRequestException("volume cannot be zero or negative");
            if (string.IsNullOrEmpty(request.InstrumentSymbol))
                throw new WalletApiBadRequestException("InstrumentSymbol cannot be empty");
            if (string.IsNullOrEmpty(request.AssetSymbol))
                throw new WalletApiBadRequestException("AssetSymbol cannot be empty");

            var walletId = this.GetWalletIdentity();

            var quote = await _swapService.GetSwapQuoteAsync(walletId, request.InstrumentSymbol,
                request.AssetSymbol, request.Volume, request.Side);

            var response = new GetSwapQuoteResponse()
            {
                InstrumentSymbol = quote.InstrumentSymbol,
                AssetSymbol = quote.AssetSymbol,
                Side = quote.OrderSide,
                Volume = quote.Volume,
                OperationId = quote.OperationId,
                Price = quote.Price,
                OppositeVolume = quote.OppositeVolume,
                OppositeAssetSymbol = quote.OppositeAssetSymbol,
                ExpireTime = quote.ExpireTime
            };

            return new Response<GetSwapQuoteResponse>(response);
        }

        /// <summary>
        /// Create limit order on the wallet
        /// </summary>
        [HttpPost("execute-quote")]
        public async Task<Response<ExecuteSwapQuoteResponse>> ExecuteSwapQuoteAsync(
            [FromBody] ExecuteSwapQuoteRequest request)
        {
            if (request == null) throw new WalletApiBadRequestException("request cannot be null");
            if (request.Volume <= 0) throw new WalletApiBadRequestException("volume cannot be zero or negative");
            if (request.Price <= 0) throw new WalletApiBadRequestException("price cannot be zero or negative");
            if (request.OppositeVolume <= 0)
                throw new WalletApiBadRequestException("opposite volume cannot be zero or negative");
            if (string.IsNullOrEmpty(request.InstrumentSymbol))
                throw new WalletApiBadRequestException("InstrumentSymbol cannot be empty");
            if (string.IsNullOrEmpty(request.AssetSymbol))
                throw new WalletApiBadRequestException("AssetSymbol cannot be empty");
            if (string.IsNullOrEmpty(request.OppositeAssetSymbol))
                throw new WalletApiBadRequestException("AssetSymbol cannot be empty");

            var walletId = this.GetWalletIdentity();

            var (executed, quote) = await _swapService.ExecuteSwapQuoteAsync(walletId, request.InstrumentSymbol,
                request.AssetSymbol, request.Side, request.OperationId, request.Volume, request.Price,
                request.OppositeVolume, request.OppositeAssetSymbol, request.ExpireTime);

            var response = new ExecuteSwapQuoteResponse()
            {
                IsExecuted = executed,
                InstrumentSymbol = quote.InstrumentSymbol,
                AssetSymbol = quote.AssetSymbol,
                Side = quote.OrderSide,
                Volume = quote.Volume,
                OperationId = quote.OperationId,
                Price = quote.Price,
                OppositeVolume = quote.OppositeVolume,
                OppositeAssetSymbol = quote.OppositeAssetSymbol,
                ExpireTime = quote.ExpireTime
            };

            return new Response<ExecuteSwapQuoteResponse>(response);
        }
    }
}