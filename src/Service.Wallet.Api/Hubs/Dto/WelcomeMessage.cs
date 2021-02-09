using System;
using DotNetCoreDecorators;
using MessagePack;

namespace Service.Wallet.Api.Hubs.Dto
{
    [SignalrOutcomming(HubNames.Welcome)]
    public class WelcomeMessage: ResponseContract<string>
    {
        
    }

    [MessagePackObject]
    public class ResponseContract
    {
        [Key("now")]
        public long Now { get; set; } = DateTime.UtcNow.UnixTime();
    }

    [MessagePackObject]
    public class ResponseContract<T> : ResponseContract
    {
        [Key("data")]
        public T Data { get; set; }

        public static ResponseContract<T> Create(T data)
        {
            return new ResponseContract<T>
            {
                Data = data
            };
        }
    }
}