using System;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class SessionRefreshRequest
    {
        public string Token { get; set; }
        public DateTime RequestTime { get; set; }

        /// <summary>
        /// Signature for request with client private key associated with root session. Algorithm:
        ///  * privateKey = Rsa.GeneratePrivateKey(2048)
        ///  * text = "{Token}_{RequestTime:yyyy-MM-ddTHH:mm:ss}_"
        ///  * buf[] = Utf8.GetBytes(text)
        ///  * hash[] = Sha256(buf)
        ///  * signature = RSA.Pkcs1.Sign(hash[], privateKey)
        /// </summary>
        public string Signature { get; set; }
    }
}