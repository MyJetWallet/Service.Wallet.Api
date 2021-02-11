using System;
using System.Collections.Generic;
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
            foreach (var connection in list)
            {
                await func.Invoke(connection);
            }
        }
    }
}