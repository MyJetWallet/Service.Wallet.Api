using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Authorization.Client.Http;
using SimpleTrading.TokensManager;
using SimpleTrading.TokensManager.Tokens;

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

        [HttpGet("who")]
        [Authorize()]
        public IActionResult Who()
        {
            var walletId = this.GetWalletIdentity();
            return Ok(walletId);
        }

        [HttpGet("token-generate")]
        public IActionResult GenerateBaseToken([FromQuery] string clientId, [FromQuery] int timeLifeMinutes)
        {
            var token = new AuthorizationToken()
            {
                Id = clientId,
                Expires = DateTime.UtcNow.AddMinutes(timeLifeMinutes)
            };

            var result = token.IssueTokenAsBase64String(Startup.SessionEncodingKey);

            return Ok(result);
        }
    }
}