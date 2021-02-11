using System.Text.Json.Serialization;

namespace Service.Wallet.Api.Domain.Models
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