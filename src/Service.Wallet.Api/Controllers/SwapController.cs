#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.AssetsDictionary.Client;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.Converter.Grpc;
using Service.Liquidity.Converter.Grpc.Models;
using Service.Service.KYC.Client;
using Service.Service.KYC.Domain.Models;
using Service.Service.KYC.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Contracts;
using Service.Wallet.Api.Domain.Wallets;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/swap")]
    public class SwapController : ControllerBase
    {
        private readonly IQuoteService _quoteService;
        private readonly IAssetsDictionaryClient _assetsDictionary;
        private readonly IKycStatusClient _kycStatusClient;
        private readonly IWalletService _walletService;

        public SwapController(IQuoteService quoteService, IAssetsDictionaryClient assetsDictionary, IKycStatusClient kycStatusClient, IWalletService walletService)
        {
            _quoteService = quoteService;
            _assetsDictionary = assetsDictionary;
            _kycStatusClient = kycStatusClient;
            _walletService = walletService;
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

            var clientId = this.GetClientIdentity();
            var walletId = await _walletService.GetDefaultWalletAsync(clientId);

            var fromAsset = _assetsDictionary.GetAssetsByBrand(clientId).FirstOrDefault(e => e.Symbol == request.FromAsset);
            var toAsset = _assetsDictionary.GetAssetsByBrand(clientId).FirstOrDefault(e => e.Symbol == request.ToAsset);

            if (fromAsset == null || toAsset == null || !fromAsset.IsEnabled || !toAsset.IsEnabled)
                throw new WalletApiBadRequestException("FromAsset or ToAsset do not found");

            //todo: https://monfex.atlassian.net/browse/SPOTDEV-227
            if (fromAsset.KycRequiredForDeposit || toAsset.KycRequiredForDeposit)
            {
                var kycStatus = _kycStatusClient.GetClientKycStatus(new KycStatusRequest()
                {
                    BrokerId = clientId.BrokerId,
                    ClientId = clientId.ClientId
                });

                if (kycStatus.Status != KycStatus.Verified)
                {
                    throw new WalletApiErrorException("KYC required ", ApiResponseCodes.KycNotPassed);
                }
            }

            var quoteResponse = await _quoteService.GetQuoteAsync(new GetQuoteRequest()
            {
                WalletId = walletId.WalletId,
                AccountId = clientId.ClientId,
                BrokerId = clientId.BrokerId,
                FromAsset = fromAsset.Symbol,
                ToAsset = toAsset.Symbol,
                FromAssetVolume = request.FromAssetVolume ?? 0.0,
                ToAssetVolume = request.ToAssetVolume ?? 0.0,
                IsFromFixed = request.IsFromFixed
            });

            if (!quoteResponse.IsSuccess || quoteResponse.Data == null)
            {
                throw new WalletApiErrorException("Can not get quote for convert", ApiResponseCodes.CannotExecuteQuoteRequest);
            }


            var response = new GetSwapQuoteResponse()
            {
                FromAsset = request.FromAsset,
                ToAsset = request.ToAsset,

                OperationId = quoteResponse.Data.OperationId,
                Price = quoteResponse.Data.Price,

                ActualTimeInSecond = (int)(quoteResponse.Data.ExpireDate - DateTime.UtcNow).TotalSeconds,

                FromAssetVolume = quoteResponse.Data.FromAssetVolume,
                ToAssetVolume = quoteResponse.Data.ToAssetVolume,

                IsFromFixed = quoteResponse.Data.IsFromFixed
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
            if (request.FromAssetVolume <= 0) throw new WalletApiBadRequestException("volume cannot be zero or negative");
            if (request.ToAssetVolume <= 0) throw new WalletApiBadRequestException("volume cannot be zero or negative");
            if (request.Price <= 0) throw new WalletApiBadRequestException("price cannot be zero or negative");
            if (string.IsNullOrEmpty(request.FromAsset))
                throw new WalletApiBadRequestException("FromAsset cannot be empty");
            if (string.IsNullOrEmpty(request.ToAsset))
                throw new WalletApiBadRequestException("ToAsset cannot be empty");
            if (string.IsNullOrEmpty(request.OperationId))
                throw new WalletApiBadRequestException("OperationId cannot be empty");

            var clientId = this.GetClientIdentity();
            var walletId = await _walletService.GetDefaultWalletAsync(clientId);

            var quoteResponse = await _quoteService.ExecuteQuoteAsync(new ExecuteQuoteRequest()
            {
                WalletId = walletId.WalletId,
                AccountId = clientId.ClientId,
                BrokerId = clientId.BrokerId,
                FromAsset = request.FromAsset,
                ToAsset = request.ToAsset,
                FromAssetVolume = request.FromAssetVolume,
                ToAssetVolume = request.ToAssetVolume,
                IsFromFixed = request.IsFromFixed,
                OperationId = request.OperationId,
                Price = request.Price
            });
            

            if (quoteResponse.QuoteExecutionResult == QuoteExecutionResult.Error)
            {
                throw new WalletApiErrorException(quoteResponse.ErrorMessage, ApiResponseCodes.CannotExecuteQuoteRequest);
            }


            var response = new ExecuteSwapQuoteResponse()
            {
                IsExecuted = quoteResponse.QuoteExecutionResult == QuoteExecutionResult.Success,

                FromAsset = quoteResponse.Data.FromAsset,
                ToAsset = quoteResponse.Data.ToAsset,

                OperationId = quoteResponse.Data.OperationId,
                Price = quoteResponse.Data.Price,

                ActualTimeInSecond = (int)(quoteResponse.Data.ExpireDate - DateTime.UtcNow).TotalSeconds,

                FromAssetVolume = quoteResponse.Data.FromAssetVolume,
                ToAssetVolume = quoteResponse.Data.ToAssetVolume,

                IsFromFixed = quoteResponse.Data.IsFromFixed
            };

            return new Response<ExecuteSwapQuoteResponse>(response);

        }
    }
}