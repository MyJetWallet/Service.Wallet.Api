using System.Collections.Generic;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class SetKeyValueRequest
    {
        public List<FrontendKeyValue.Domain.Models.FrontKeyValue> Keys { get; set; }
    }
}