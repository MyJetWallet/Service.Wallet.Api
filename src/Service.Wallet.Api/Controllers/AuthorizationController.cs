using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Authorization.Client.Http;
using Service.Authorization.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Contracts;
using AuthorizationRequest = Service.Wallet.Api.Controllers.Contracts.AuthorizationRequest;
using IAuthorizationService = Service.Authorization.Grpc.IAuthorizationService;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Route("/api/v1/authorization")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpPost("authorization")]
        public async Task<Response<string>> AuthorizationAsync(AuthorizationRequest request)
        {
            var response = await _authorizationService.AuthorizationAsync(new Authorization.Grpc.Models.AuthorizationRequest()
            {
                BrandId = "default-brand",
                BrokerId = Program.Settings.BrokerId,
                WalletId = null,
                PublicKeyPem = request.PublicKeyPem,
                Token = request.AuthToken,
                UserAgent = this.HttpContext.GetUserAgent()
            });

            if (!response.Result)
            {
                throw new WalletApiHttpException("Authorization Fail", HttpStatusCode.Unauthorized);
            }

            return new Response<string>(response.Token);
        }

        [HttpPost("refresh")]
        public async Task<Response<string>> RefreshSessionAsync(SessionRefreshRequest request)
        {
            var signature = request.Signature;

            if (string.IsNullOrEmpty(signature))
            {
                throw new WalletApiHttpException("Signature is empty", HttpStatusCode.Unauthorized);
            }

            var response = await _authorizationService.RefreshSessionAsync(new Authorization.Grpc.Models.RefreshSessionRequest()
            {
                UserAgent = this.HttpContext.GetUserAgent(),
                Ip = this.HttpContext.GetIp(),

                Token = request.Token,
                NewWalletId = null,
                RequestTimestamp = request.RequestTime,
                SignatureBase64 = signature
            });

            if (!response.Result)
            {
                throw new WalletApiHttpException("Authorization Fail", HttpStatusCode.Unauthorized);
            }

            return new Response<string>(response.Token);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<Response> LogoutAsync()
        {
            var (sessionRootId, sessionId) = this.GetSessionId();

            await _authorizationService.KillRootSessionAsync(new KillRootSessionRequest()
            {
                Reason = "logout",
                ClientId = this.GetClientId(),
                UserAgent = this.HttpContext.GetUserAgent(),
                Ip = this.HttpContext.GetIp(),
                SessionId = sessionId,
                SessionRootId = sessionRootId
            });

            return new Response(ApiResponseCodes.OK);
        }
    }
}