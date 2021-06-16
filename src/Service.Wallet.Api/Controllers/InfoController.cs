using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Sdk.Authorization.Http;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Wallets;

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
    }
}