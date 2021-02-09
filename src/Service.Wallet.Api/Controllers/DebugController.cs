using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Route("/api/Debug")]
    public class DebugController: ControllerBase
    {
        /// <summary>
        /// Parse token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("token/{token}")]
        public IActionResult ParseToken([FromRoute] string token)
        {
            var result = token.PrintToken();

            return Ok(result);
        }

        [HttpGet("hello")]
        public IActionResult HelloWorld()
        {
            return Ok("Hello world!");
        }


        [HttpGet("test")]
        [Authorize()]
        public IActionResult TestAuth()
        {
            var traderId = User.Identity.Name;
            return Ok($"Hello {traderId}");
        }
    }
}