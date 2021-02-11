using System.Text.Json.Serialization;

namespace Service.Wallet.Api.Controllers.Contracts
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApiResponseCodes
    {
        OK
    }
}