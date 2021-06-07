using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Authorization.Http;
using Serilog;
using Service.PushNotification.Domain.Models;
using Service.PushNotification.Grpc;
using Service.Wallet.Api.Controllers.Contracts;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Route("/api/v1/push")]
    public class PushNotificationController:ControllerBase
    {

        private readonly ITokenManager _tokenManager;
        private readonly ILogger<PushNotificationController> _logger;

        public PushNotificationController(ILogger<PushNotificationController> logger, ITokenManager tokenManager)
        {
            _logger = logger;
            _tokenManager = tokenManager;
        }


        /// <summary>
        /// Token registration for push notification  
        /// </summary>
        [HttpPost("token")]
        [Authorize]
        public async Task<Response> RegisterToken([FromBody] RegisterTokenRequest request)
        {
            var identity = this.GetClientIdentity();
            var (rootSessionId, _) = this.GetSessionId();
            
            await _tokenManager.RegisterToken(new PushToken()
            {
                ClientId = identity.ClientId,
                RootSessionId = rootSessionId,
                Token = request.Token,
                UserLocale = request.UserLocale
            });
            _logger.LogInformation("Received token {Token} and locale {UserLocale}", request.Token, request.UserLocale);
            return Contracts.Response.OK();
        }
    }
}