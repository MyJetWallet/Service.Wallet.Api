using System.Runtime.Serialization;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}