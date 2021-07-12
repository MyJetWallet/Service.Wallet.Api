using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Sdk.Authorization.Http;
using Service.FrontendKeyValue.Domain.Models;
using Service.FrontendKeyValue.Grpc;
using Service.FrontendKeyValue.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/key-value")]
    public class KeyValueController : ControllerBase
    {
        private readonly IFrontKeyValueService _frontKeyValueService;

        public KeyValueController(IFrontKeyValueService frontKeyValueService)
        {
            _frontKeyValueService = frontKeyValueService;
        }

        [HttpPost("set")]
        public async Task SetKeyValue([FromBody] SetKeyValueRequest body)
        {
            var clientId = this.GetClientId();

            await _frontKeyValueService.SetKeysAsync(new SetFrontKeysRequest()
            {
                ClientId = clientId,
                KeyValues = body.Keys ?? new List<FrontKeyValue>()
            });
        }

        [HttpPost("remove")]
        public async Task SetKeyValue([FromBody] List<string> keys)
        {
            var clientId = this.GetClientId();

            await _frontKeyValueService.DeleteKeysAsync(new DeleteFrontKeysRequest()
            {
                Keys = keys ?? new List<string>(),
                ClientId = clientId
            });
        }

        [HttpGet("debug/get")]
        public async Task<List<FrontKeyValue>> GetKeyValue()
        {
            var clientId = this.GetClientId();

            var data = await _frontKeyValueService.GetKeysAsync(new GetFrontKeysRequest()
            {
                ClientId = clientId
            });

            return data.KeyValues;
        }
    }
}