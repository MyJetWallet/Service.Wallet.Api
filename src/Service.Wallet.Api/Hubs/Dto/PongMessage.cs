namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.Pong)]
    public class PongMessage : MesssageContract
    {
    }
}