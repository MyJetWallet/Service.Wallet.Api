using System.ServiceModel;
using System.Threading.Tasks;
using Service.AssetsDictionary.Client;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.WithdrawalProcessor.Grpc.Models;

namespace Service.Wallet.Api.Services
{
    public interface IBlockchainIntegrationService
    {
        Task<GetDepositAddressResponse> GetDepositAddressAsync(GetDepositAddressRequest request);

        Task<ValidateAddressResponse> ValidateAddressAsync(ValidateAddressRequest request);

        Task<CryptoWithdrawalResponse> CryptoWithdrawalAsync(CryptoWithdrawalRequest request);
    }
}