using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Service.Wallet.Api.Hubs
{
    public class HubClientConnections
    {
        //ToDo - Improve search algos. We are searching by TraderId and by Instruments
        private readonly Dictionary<string, HubClientConnection> _connections =
            new Dictionary<string, HubClientConnection>();

        private IReadOnlyList<HubClientConnection> _allConnections = new List<HubClientConnection>();

        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

        public int Count => _allConnections.Count;

        private void UpdateAllConnectionsCache()
        {
            _allConnections = _connections.Values.ToList();
        }

        public void Connected(HubClientConnection connection)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                if (_connections.ContainsKey(connection.ConnectionId))
                    _connections.Remove(connection.ConnectionId);

                _connections.Add(connection.ConnectionId, connection);

                UpdateAllConnectionsCache();

            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }


        public void Disconnected(string connectionId)
        {

            _lockSlim.EnterWriteLock();
            try
            {
                if (_connections.ContainsKey(connectionId))
                    _connections.Remove(connectionId);

                UpdateAllConnectionsCache();
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public HubClientConnection Get(string connectionId)
        {
            _lockSlim.EnterReadLock();
            try
            {
                if (_connections.ContainsKey(connectionId))
                    return _connections[connectionId];

                throw new Exception($"Connection not found with ID:{connectionId}");
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public HubClientConnection TryGet(string connectionId)
        {
            _lockSlim.EnterReadLock();
            try
            {
                return _connections.ContainsKey(connectionId) ? _connections[connectionId] : null;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }


        public IEnumerable<HubClientConnection> GetByCondition(Func<HubClientConnection, bool> predicate)
        {
            var allConnections = _allConnections;
            return allConnections.Where(predicate);
        }



        public IReadOnlyList<HubClientConnection> GetAll()
        {
            return _allConnections;
        }
    }
}