using System;
using System.Net;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class WalletApiHttpException: Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public WalletApiHttpException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}