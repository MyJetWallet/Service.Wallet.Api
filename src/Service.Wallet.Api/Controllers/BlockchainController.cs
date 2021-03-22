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
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.DepositDetector.Grpc;
using Service.Bitgo.WithdrawalProcessor.Grpc;
using Service.Bitgo.WithdrawalProcessor.Grpc.Models;
using Service.Service.KYC.Client;
using Service.Service.KYC.Domain.Models;
using Service.Service.KYC.Grpc.Models;
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
        private readonly IBitgoDepositAddressService _addressService;
        private readonly IKycStatusClient _kycStatusClient;

        public BlockchainController(ILogger<BlockchainController> logger, 
            ICryptoWithdrawalService cryptoWithdrawalService,
            IAssetsDictionaryClient assetsDictionaryClient,
            IWalletBalanceService balanceService,
            IBitgoDepositAddressService addressService,
            IKycStatusClient kycStatusClient)
        {
            _logger = logger;
            _cryptoWithdrawalService = cryptoWithdrawalService;
            _assetsDictionaryClient = assetsDictionaryClient;
            _balanceService = balanceService;
            _addressService = addressService;
            _kycStatusClient = kycStatusClient;
        }

        /// <summary>
        /// Generate crypto deposit address
        /// </summary>
        [HttpPost("generate-deposit-address")]
        public async Task<Response<GenerateDepositAddressResponse>> GenerateDepositAddressAsync(GenerateDepositAddressRequest request)
        {
            var walletId = await HttpContext.GetWalletIdentityAsync(request.WalletId);

            _logger.LogInformation("Receive Generate deposit address. User: {brokerId}|{clientId}. Request: {jsonText}",
                walletId.BrokerId, walletId.ClientId, JsonSerializer.Serialize(request));

            var asset = _assetsDictionaryClient.GetAssetById(new AssetIdentity()
            {
                BrokerId = walletId.BrokerId,
                Symbol = request.AssetSymbol
            });

            if (asset == null)
                throw new WalletApiErrorException("Asset do not found", ApiResponseCodes.AssetDoNotFound);

            if (!asset.IsEnabled)
                throw new WalletApiErrorException("Asset is disabled", ApiResponseCodes.AssetIsDisabled);

            if (asset.KycRequiredForDeposit)
            {
                var kycStatus = _kycStatusClient.GetClientKycStatus(new KycStatusRequest()
                {
                    BrokerId = walletId.BrokerId,
                    ClientId = walletId.ClientId
                });

                if (kycStatus.Status != KycStatus.Verified)
                    throw new WalletApiErrorException("KYC is required", ApiResponseCodes.KycNotPassed);
            }

            var result = await _addressService.GetDepositAddressAsync(new GetDepositAddressRequest()
            {
                BrokerId = walletId.BrokerId,
                WalletId = walletId.WalletId,
                ClientId = walletId.ClientId,
                AssetSymbol = asset.Symbol
            });


            if (result.Error == GetDepositAddressResponse.ErrorCode.AssetDoNotSupported)
                throw new WalletApiErrorException("Crypto withdrawal do not supported for asset", ApiResponseCodes.AssetDoNotSupported);

            //todo: получить отдельно адрес, отдельно мемо и тип мемо
            return new Response<GenerateDepositAddressResponse>(new GenerateDepositAddressResponse()
            {
                Address = result.Address
            });
        }

        /// <summary>
        /// execute crypto withdrawal
        /// </summary>
        [HttpPost("withdrawal")]
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

            if (asset.KycRequiredForDeposit)
            {
                var kycStatus = _kycStatusClient.GetClientKycStatus(new KycStatusRequest()
                {
                    BrokerId = walletId.BrokerId,
                    ClientId = walletId.ClientId
                });

                if (kycStatus.Status != KycStatus.Verified)
                    throw new WalletApiErrorException("KYC is required", ApiResponseCodes.KycNotPassed);
            }


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