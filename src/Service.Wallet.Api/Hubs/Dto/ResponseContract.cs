using System;
using DotNetCoreDecorators;
using MessagePack;

namespace Service.Wallet.Api.Hubs.Dto
{
    //[MessagePackObject]
    public class MesssageContract
    {
        //[Key("now")]
        public long Now { get; set; } = DateTime.UtcNow.UnixTime();
    }

    //[MessagePackObject]
    public class MesssageContract<T> : MesssageContract
    {
        //[Key("data")]
        public T Data { get; set; }

        public static MesssageContract<T> Create(T data)
        {
            return new MesssageContract<T>
            {
                Data = data
            };
        }
    }

}