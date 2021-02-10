using System.Text.Json.Serialization;

namespace Service.Wallet.Api.Controllers.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        Unknown,
        Placed,
        Filled,
        Canceled
    }
}