using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Wallet.Api.Hubs
{
    public interface IHubManager
    {
        void Connected(HubClientConnection ctx);
        HubClientConnection TryGetContext(string connectionId);
        void Disconnected(string connectionId);
        IReadOnlyList<HubClientConnection> GetAllConnections();

        Task ExecForeachConnection(Func<HubClientConnection, Task> func);

        IEnumerable<HubClientConnection> TryGetContextByWalletId(string walletId);
    }

    public class HubManager: IHubManager
    {
        private readonly HubClientConnections _hubConnections = new HubClientConnections();

        public void Connected(HubClientConnection ctx)
        {
            _hubConnections.Connected(ctx);
        }

        public HubClientConnection TryGetContext(string connectionId)
        {
            return _hubConnections.TryGet(connectionId);
        }

        public void Disconnected(string connectionId)
        {
            _hubConnections.Disconnected(connectionId);
        }

        public IReadOnlyList<HubClientConnection> GetAllConnections()
        {
            return _hubConnections.GetAll();
        }

        public async Task ExecForeachConnection(Func<HubClientConnection, Task> func)
        {
            var list = _hubConnections.GetAll();
            var taskList = new List<Task>(list.Count);

            foreach (var connection in list)
            {
                taskList.Add(func.Invoke(connection));
            }

            await Task.WhenAll(taskList);
        }

        public IEnumerable<HubClientConnection> TryGetContextByWalletId(string walletId)
        {
            //todo: need to optimize, add dictionary by wallet id
            var data = _hubConnections.GetByCondition(e => e?.WalletId.WalletId == walletId);
            return data;
        }
    }
}