using Autofac;
using DotNetCoreDecorators;
using MyJetWallet.Domain.Prices;
using MyJetWallet.Domain.ServiceBus.PublisherSubscriber.BidAsks;
using MyJetWallet.Sdk.Service;
using MyServiceBus.TcpClient;

namespace Service.Wallet.Api.Modules
{
    public class ServiceBusModule : Module
    {
        private readonly MyServiceBusTcpClient _serviceBusClient;

        public ServiceBusModule(MyServiceBusTcpClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var queue = $"WalletAPi-{ApplicationEnvironment.HostName}";
            builder.RegisterInstance(new BidAskMyServiceBusSubscriber(_serviceBusClient, queue, true))
                .As<ISubscriber<BidAsk>>()
                .SingleInstance();
        }
    }
}