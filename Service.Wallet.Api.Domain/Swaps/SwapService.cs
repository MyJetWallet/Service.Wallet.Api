using System;
using System.Threading.Tasks;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Assets;
using MyJetWallet.Domain.Orders;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;
using Service.Liquidity.Converter.Grpc;
using Service.Liquidity.Converter.Grpc.Models;
using Service.Service.KYC.Client;
using Service.Service.KYC.Domain.Models;
using Service.Service.KYC.Grpc.Models;
using Service.Wallet.Api.Domain.Contracts;

namespace Service.Wallet.Api.Domain.Swaps
{
    public class SwapService : ISwapService
    {
        private readonly ISpotInstrumentDictionaryClient _spotInstrumentDictionaryClient;
        private readonly IAssetsDictionaryClient _assetsDictionaryClient;
        private readonly IKycStatusClient _kycStatusClient;
        private readonly IQuoteService _quoteService;

        public SwapService(ISpotInstrumentDictionaryClient spotInstrumentDictionaryClient,
            IAssetsDictionaryClient assetsDictionaryClient, IKycStatusClient kycStatusClient,
            IQuoteService quoteService)
        {
            _spotInstrumentDictionaryClient = spotInstrumentDictionaryClient;
            _assetsDictionaryClient = assetsDictionaryClient;
            _kycStatusClient = kycStatusClient;
            _quoteService = quoteService;
        }

        public async Task<Quote> GetSwapQuoteAsync(
            IJetWalletIdentity walletId,
            string symbol,
            string assetSymbol,
            double volume,
            OrderSide side)
        {
            var spotInstrument = _spotInstrumentDictionaryClient.GetSpotInstrumentById(new SpotInstrumentIdentity()
            {
                BrokerId = walletId.BrokerId,
                Symbol = symbol
            });

            var asset = _assetsDictionaryClient.GetAssetById(new AssetIdentity()
            {
                BrokerId = walletId.BrokerId,
                Symbol = assetSymbol,
            });

            ValidateInstrument(spotInstrument);
            ValidateAsset(asset);
            ValidateKyc(spotInstrument, walletId.BrokerId, walletId.ClientId);

            var quoteResponse = await _quoteService.GetQuote(new QuoteRequest()
            {
                BrokerId = walletId.BrokerId,
                AccountId = walletId.ClientId,
                WalletId = walletId.WalletId,
                AssetSymbol = assetSymbol,
                InstrumentSymbol = symbol,
                OrderSide = side,
                Volume = volume,
            });

            if (!quoteResponse.IsSuccess)
            {
                RejectSwapRequest(ApiResponseCodes.CannotProcessQuoteRequest, quoteResponse.ErrorMessage);
            }

            return quoteResponse.Data;
        }

        public async Task<(bool, Quote)> ExecuteSwapQuoteAsync(
            IJetWalletIdentity walletId,
            string operationId)
        {
            var swapResponse = await _quoteService.ExecuteQuote(new Quote()
            {
                OperationId = operationId
                //todo: передать все параметры запроса для даблчека
            });

            if (swapResponse.QuoteExecutionResult == QuoteExecutionResult.Error)
            {
                RejectSwapRequest(ApiResponseCodes.CannotExecuteQuoteRequest, swapResponse.ErrorMessage);
            }

            return (swapResponse.QuoteExecutionResult == QuoteExecutionResult.Success, swapResponse.Data);
        }

        private void ValidateInstrument(ISpotInstrument spotInstrument)
        {
            if (spotInstrument == null)
            {
                RejectSwapRequest(ApiResponseCodes.InvalidInstrument, "Unknown instrument.");
                return;
            }

            if (!spotInstrument.IsEnabled)
            {
                RejectSwapRequest(ApiResponseCodes.InvalidInstrument, "Disabled instrument.");
            }
        }

        private void ValidateAsset(IAsset asset)
        {
            if (asset == null)
            {
                RejectSwapRequest(ApiResponseCodes.AssetDoNotFound, "Unknown asset.");
                return;
            }

            if (!asset.IsEnabled)
            {
                RejectSwapRequest(ApiResponseCodes.AssetIsDisabled, "Disabled asset.");
            }
        }

        private void ValidateKyc(ISpotInstrument spotInstrument, string brokerId, string clientId)
        {
            if (!spotInstrument.KycRequiredForTrade) return;
            var kycStatus = _kycStatusClient.GetClientKycStatus(new KycStatusRequest()
            {
                BrokerId = brokerId,
                ClientId = clientId
            });

            if (kycStatus != null && kycStatus.Status != KycStatus.Verified)
            {
                RejectSwapRequest(ApiResponseCodes.KycNotPassed, "KYC not passed.");
            }
        }

        private void RejectSwapRequest(ApiResponseCodes code, string statusReason)
        {
            throw new WalletApiErrorException(statusReason, code);
        }
    }
}