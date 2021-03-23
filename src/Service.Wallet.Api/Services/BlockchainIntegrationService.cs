using System.Threading.Tasks;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.DepositDetector.Grpc;
using Service.Bitgo.WithdrawalProcessor.Grpc;
using Service.Bitgo.WithdrawalProcessor.Grpc.Models;

namespace Service.Wallet.Api.Services
{
    public class BlockchainIntegrationService : IBlockchainIntegrationService
    {
        private readonly ICryptoWithdrawalService _cryptoWithdrawalService;
        private readonly IBitgoDepositAddressService _addressService;

        public BlockchainIntegrationService(
            ICryptoWithdrawalService cryptoWithdrawalService,
            IBitgoDepositAddressService addressService)
        {
            _cryptoWithdrawalService = cryptoWithdrawalService;
            _addressService = addressService;
        }

        public Task<GetDepositAddressResponse> GetDepositAddressAsync(GetDepositAddressRequest request)
        {
            return _addressService.GetDepositAddressAsync(request);
        }

        public Task<ValidateAddressResponse> ValidateAddressAsync(ValidateAddressRequest request)
        {
            return _cryptoWithdrawalService.ValidateAddressAsync(request);
        }

        public Task<CryptoWithdrawalResponse> CryptoWithdrawalAsync(CryptoWithdrawalRequest request)
        {
            return _cryptoWithdrawalService.CryptoWithdrawalAsync(request);
        }
    }
}