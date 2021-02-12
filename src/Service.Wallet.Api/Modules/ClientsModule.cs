using Autofac;
using MyNoSqlServer.DataReader;
using Service.AssetsDictionary.Client;
using Service.ClientWallets.Client;
using Service.MatchingEngine.PriceSource.Client;

namespace Service.Wallet.Api.Modules
{
    public class ClientsModule: Module
    {
        private readonly MyNoSqlTcpClient _myNoSqlClient;

        public ClientsModule(MyNoSqlTcpClient myNoSqlClient)
        {
            _myNoSqlClient = myNoSqlClient;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssetsDictionaryClients(_myNoSqlClient);

            builder.RegisterClientWalletsClients(_myNoSqlClient, Program.Settings.ClientWalletsGrpcServiceUrl);

            builder.RegisterMatchingEnginePriceSourceClient(_myNoSqlClient);
        }
    }
}