using MessagePack;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.Welcome)]
    public class WelcomeMessage: MesssageContract<string>
    {
        
    }
}