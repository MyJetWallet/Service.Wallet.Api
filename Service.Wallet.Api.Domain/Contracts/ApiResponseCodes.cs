using System.Text.Json.Serialization;

namespace Service.Wallet.Api.Domain.Contracts
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApiResponseCodes
    {
        OK = 0,
        InternalServerError = 1,
        WalletDoNotExist = 2,
        LowBalance = 3,
        CannotProcessWithdrawal = 4,
        AddressIsNotValid = 5,
        AssetDoNotFound = 6,
        AssetIsDisabled = 7,
        AmountIsSmall = 8,
        InvalidInstrument = 9,
        KycNotPassed = 10,
        AssetDoNotSupported = 11,
        NotEnoughLiquidityForMarketOrder = 12,
        InvalidOrderValue = 13,
        CannotProcessQuoteRequest = 14,
        CannotExecuteQuoteRequest = 15,
        NoqEnoughLiquidityForConvert = 16
    }
}