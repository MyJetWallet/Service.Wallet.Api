using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Wallet.Api.Controllers.Contracts;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Route("/api/v1/push")]
    public class PushNotificationController:ControllerBase
    {
        /// <summary>
        /// Placeholder for push notification token registration 
        /// </summary>
        [HttpPost("token")]
        [Authorize]
        public async Task<Response> RegisterToken([FromBody] RegisterTokenRequest request)
        {
            return Contracts.Response.OK();
        }
    }
}