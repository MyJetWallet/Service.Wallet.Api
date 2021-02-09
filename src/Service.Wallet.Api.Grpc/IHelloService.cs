using System.ServiceModel;
using System.Threading.Tasks;
using Service.Wallet.Api.Grpc.Models;

namespace Service.Wallet.Api.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}