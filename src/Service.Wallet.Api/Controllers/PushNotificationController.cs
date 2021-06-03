using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using Service.Wallet.Api.Controllers.Contracts;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Route("/api/v1/push")]
    public class PushNotificationController:ControllerBase
    {
        
        private readonly ILogger<PushNotificationController> _logger;

        public PushNotificationController(ILogger<PushNotificationController> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// Placeholder for push notification token registration 
        /// </summary>
        [HttpPost("token")]
        [Authorize]
        public async Task<Response> RegisterToken([FromBody] RegisterTokenRequest request)
        {
            _logger.LogInformation("Received token {Token} and locale {UserLocale}", request.Token, request.UserLocale);
            return Contracts.Response.OK();
        }
    }
}