using System.Text.Json.Serialization;

namespace Service.Wallet.Api.Domain.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Direction
    {
        Unknown = 0,
        Buy = 1,
        Sell
    }
}