using System.Text.Json.Serialization;

namespace Service.Wallet.Api.Domain.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BalanceHistoryType
    {
        Deposit,
        Withdrawal,
        Trade,
        Transfer,
        Convert
    }
}