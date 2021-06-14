using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.Authorization;
using MyJetWallet.Sdk.Authorization.Http;
using MyTcpSockets.Extensions;
using Newtonsoft.Json;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Common;
using SimpleTrading.ClientApi.Utils;
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
        [HttpPost("token")]
        public IActionResult ParseToken([FromBody] TokenDto request)
        {
            var (res, data) = MyControllerBaseHelper.ParseToken(request.Token);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);

            return Ok($"Result: {res}.\nData:\n{json}");
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
            var traderId = this.GetClientId();
            return Ok($"Hello {traderId}");
        }

        [HttpGet("who")]
        [Authorize()]
        public IActionResult Who()
        {
            var clientId = this.GetClientId();
            var brokerId = this.GetBrokerId();
            var brandId = this.GetBrandId();
            return Ok(new JetClientIdentity(brokerId, brandId, clientId));
        }

        [HttpPost("make-signature")]
        public IActionResult MakeSignatureAsync([FromBody] TokenDto data, [FromHeader(Name = "private-key")] string key)
        {
            return Ok();
        }

        [HttpPost("generate-keys")]
        public IActionResult GenerateKeysAsync()
        {
            var rsa = RSA.Create();

            var publicKey = rsa.ExportRSAPublicKey();
            var privateKey = rsa.ExportRSAPrivateKey();

            var response = new
            {
                PrivateKeyBase64 = Convert.ToBase64String(privateKey),
                PublicKeyBase64 = Convert.ToBase64String(publicKey)
            };

            return Ok(response);
        }

        [HttpPost("validate-signature")]
        [Authorize]
        public IActionResult ValidateSignatureAsync([FromBody] TokenDto data, [FromHeader(Name = AuthorizationConst.SignatureHeader)] string signature)
        {
            return Ok();
        }

        [HttpGet("my-ip")]
        public IActionResult GetMyApiAsync()
        {
            var ip = this.HttpContext.GetIp();
            
            var xff = HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var xffheader) ? xffheader.ToString() : "none";
            var cf = HttpContext.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfheader) ? cfheader.ToString() : "none";
            
            return Ok(new {IP = ip, XFF = xff, CF = cf});
        }

        [HttpGet("verified-email-only")]        
        [Authorize(Policy = AuthorizationPolicies.VerifiedEmailPolicy)]
        public IActionResult WhoWithVerifiedEmail()
        {
            var clientId = this.GetClientId();
            var brokerId = this.GetBrokerId();
            var brandId = this.GetBrandId();
            return Ok(new JetClientIdentity(brokerId, brandId, clientId));
        }
    }
}