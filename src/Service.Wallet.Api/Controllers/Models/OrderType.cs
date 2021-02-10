using System.Text.Json.Serialization;

namespace Service.Wallet.Api.Controllers.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderType
    {
        Unknown = 0,
        Limit = 1,
        Market = 2
    }
}