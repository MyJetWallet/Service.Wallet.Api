using System;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.Assets;
using Service.AssetsDictionary.Client;
using Service.Balances.Grpc;
using Service.Bitgo.WithdrawalProcessor.Grpc;
using Service.Bitgo.WithdrawalProcessor.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Contracts;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/blockchain")]
    public class BlockchainController : ControllerBase
    {
        private readonly ILogger<BlockchainController> _logger;
        private readonly ICryptoWithdrawalService _cryptoWithdrawalService;
        private readonly IAssetsDictionaryClient _assetsDictionaryClient;
        private readonly IWalletBalanceService _balanceService;

        public BlockchainController(ILogger<BlockchainController> logger, 
            ICryptoWithdrawalService cryptoWithdrawalService,
            IAssetsDictionaryClient assetsDictionaryClient,
                IWalletBalanceService balanceService)
        {
            _logger = logger;
            _cryptoWithdrawalService = cryptoWithdrawalService;
            _assetsDictionaryClient = assetsDictionaryClient;
            _balanceService = balanceService;
        }

        public async Task<Response<GenerateDepositAddressResponse>> GenerateDepositAddressAsync(GenerateDepositAddressRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<Response<WithdrawalResponse>> WithdrawalAsync(WithdrawalRequest request)
        {
            var walletId = await HttpContext.GetWalletIdentityAsync(request.WalletId);

            var requestId = request.RequestId ?? Guid.NewGuid().ToString("N");

            _logger.LogInformation("Receive Crypto Withdrawal User: {brokerId}|{clientId}. RequestId: {requestId}. Request: {jsonText}", 
                walletId.BrokerId, walletId.ClientId, requestId, JsonSerializer.Serialize(request));

            var asset = _assetsDictionaryClient.GetAssetById(new AssetIdentity()
            {
                BrokerId =walletId.BrokerId,
                Symbol = request.AssetSymbol
            });


            // ------- validations ------- 

            if (asset == null)
                throw new WalletApiErrorException("Asset do not found", ApiResponseCodes.AssetDoNotFound);

            if (!asset.IsEnabled)
                throw new WalletApiErrorException("Asset is disabled", ApiResponseCodes.AssetIsDisabled);

            var amount = Math.Round(request.Amount, asset.Accuracy);
            var minAmount = 0; //todo: получить минимальный баланс для вывода из настроек

            if (amount <= minAmount)
                throw new WalletApiErrorException($"Amount is small. Min amount should be {minAmount}", ApiResponseCodes.AmountIsSmall);

            var balance = await _balanceService.GetBalancesByWalletAndSymbol(walletId.WalletId, asset.Symbol);

            if (balance.Balance - balance.Reserve - amount <= -Double.Epsilon)
                throw new WalletApiErrorException("Low balance", ApiResponseCodes.LowBalance);

            
            // ------- execute ------- 

            var result = await _cryptoWithdrawalService.CryptoWithdrawalAsync(new CryptoWithdrawalRequest()
            {
                BrokerId = walletId.BrokerId,
                ClientId = walletId.ClientId,
                WalletId = walletId.WalletId,
                AssetSymbol = request.AssetSymbol,
                Amount = request.Amount,
                RequestId = requestId,
                ToAddress = request.ToAddress
            });

            if (result.Error == null || result.Error.Code == BitgoErrorType.ErrorCode.Ok)
            { 
                _logger.LogInformation("Crypto Withdrawal is done. User: {brokerId}|{clientId}. RequestId: {requestId}. Result: {jsonText}",
                walletId.BrokerId, walletId.ClientId, requestId, JsonSerializer.Serialize(request));
            }
            else
            {
                _logger.LogWarning("Crypto Withdrawal is FAIL. User: {brokerId}|{clientId}. RequestId: {requestId}. Result: {jsonText}",
                    walletId.BrokerId, walletId.ClientId, requestId, JsonSerializer.Serialize(request));
            }
            
            switch (result.Error?.Code)
            {
                case BitgoErrorType.ErrorCode.LowBalance:
                case BitgoErrorType.ErrorCode.BalanceNotEnough:
                    throw new WalletApiErrorException("Low balance", ApiResponseCodes.LowBalance);

                case BitgoErrorType.ErrorCode.AssetIsNotFoundInBitGo:
                    throw new WalletApiErrorException("Crypto withdrawal for asset do not supported", ApiResponseCodes.CannotProcessWithdrawal);

                case BitgoErrorType.ErrorCode.AddressIsNotValid:
                    throw new WalletApiErrorException("Destination address is not valid", ApiResponseCodes.AddressIsNotValid);

                case BitgoErrorType.ErrorCode.InternalError:
                case BitgoErrorType.ErrorCode.AssetDoNotFound:
                case BitgoErrorType.ErrorCode.AssetIsDisabled:
                    throw new WalletApiErrorException(result.Error.Message, ApiResponseCodes.InternalServerError);
            }

            return new Response<WithdrawalResponse>(new WithdrawalResponse()
            {
                OperationId = result.OperationId,
                TxId = result.TxId,
                TxUrl = null //todo: сделать мапу урлов в nosql для отсылки на транзакцию
            });
        }
    }
}