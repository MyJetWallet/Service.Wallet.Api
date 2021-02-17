using System.Net;

namespace Service.Wallet.Api.Domain.Contracts
{
    public class WalletApiBadRequestException: WalletApiHttpException
    {
        public WalletApiBadRequestException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}