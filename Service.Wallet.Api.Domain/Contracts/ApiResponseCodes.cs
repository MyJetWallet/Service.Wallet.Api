using System.Text.Json.Serialization;

namespace Service.Wallet.Api.Domain.Contracts
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApiResponseCodes
    {
        OK = 0,
        InternalServerError = 1
    }
}