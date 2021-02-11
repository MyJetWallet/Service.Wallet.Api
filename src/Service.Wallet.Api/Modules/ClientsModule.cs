using Autofac;
using MyNoSqlServer.DataReader;
using Service.AssetsDictionary.Client;

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
        }
    }
}