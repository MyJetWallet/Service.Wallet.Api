using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Sdk.Authorization.Http;
using MyJetWallet.Sdk.WalletApi;
using MyJetWallet.Sdk.WalletApi.Contracts;
using MyJetWallet.Sdk.WalletApi.Wallets;
using Service.Wallet.Api.Controllers.Contracts;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/info")]
    public class InfoController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public InfoController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Create limit order on the wallet
        /// Errors:
        ///  * NoqEnoughLiquidityForConvert
        /// </summary>
        [HttpGet("session-info")]
        public async Task<Response<GetSessionInfoResponse>> GetSessionInfoAsync()
        {
            await _walletService.GetDefaultWalletAsync(this.GetClientIdentity());

            var tokenStr = this.GetSessionToken();
            var (_, token) = MyControllerBaseHelper.ParseToken(tokenStr);

            var response = new GetSessionInfoResponse()
            {
                EmailVerified = token.EmailVerified,
                PhoneVerified = false,
                TwoFactorAuthentication = token.Passed2FA,
                TokenLifetimeRemaining = (token.Expires - DateTime.UtcNow).ToString()
            };

            return new Response<GetSessionInfoResponse>(response);
        }
        
        [AllowAnonymous]
        [HttpGet("server-time")]
        public DateTime GetServerTime() => DateTime.UtcNow;
        
        
    }
}