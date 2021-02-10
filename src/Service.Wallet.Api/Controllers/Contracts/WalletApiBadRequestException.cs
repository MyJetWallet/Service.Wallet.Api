using System.Net;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class WalletApiBadRequestException: WalletApiHttpException
    {
        public WalletApiBadRequestException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}