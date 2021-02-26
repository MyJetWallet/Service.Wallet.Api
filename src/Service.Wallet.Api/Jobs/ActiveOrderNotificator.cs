using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.Abstractions;
using Service.ActiveOrders.Domain.Models;
using Service.Wallet.Api.Hubs;

namespace Service.Wallet.Api.Jobs
{
    public class ActiveOrderNotificator : IDisposable
    {
        private readonly IHubManager _hubManager;
        private readonly IMyNoSqlServerDataReader<OrderNoSqlEntity> _reader;
        private readonly ILogger<ActiveOrderNotificator> _logger;

        private Dictionary<string, string> _changedWallets = new Dictionary<string, string>();
        private readonly object _gate = new object();

        private readonly MyTaskTimer _timer;

        public ActiveOrderNotificator(IHubManager hubManager, IMyNoSqlServerDataReader<OrderNoSqlEntity> reader,
            ILogger<ActiveOrderNotificator> logger)
        {
            _hubManager = hubManager;
            _reader = reader;
            _logger = logger;
            _timer = MyTaskTimer.Create<ActiveOrderNotificator>(TimeSpan.FromMilliseconds(500), logger, DoProcess);
            reader.SubscribeToUpdateEvents(HandleUpdate, HandleDelete);
        }

        private void HandleUpdate(IReadOnlyList<OrderNoSqlEntity> entities)
        {
            lock (_gate)
            {
                foreach (var entity in entities)
                {
                    _changedWallets[entity.PartitionKey] = entity.PartitionKey;
                }
            }
        }

        private void HandleDelete(IReadOnlyList<OrderNoSqlEntity> entities)
        {
        }

        public async Task DoProcess()
        {
            var sw = new Stopwatch();
            sw.Start();

            var changes = GetChanges();
            
            if (!changes.Any())
                return;

            var countSent = 0;
            foreach (var walletId in changes.Keys)
            {
                var contexts = _hubManager.TryGetContextByWalletId(walletId);
                foreach (var context in contexts)
                {
                    try
                    {
                        countSent++;
                        await context.SendActiveOrdersAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Cannot send active orders to {walletId}", walletId);
                    }
                }
            }

            sw.Stop();
            if (countSent > 0)
            {
                _logger.LogDebug("Active order updates. Count: {count}, Time: {ElapsedMilliseconds} ms",
                    countSent, sw.ElapsedMilliseconds);
            }
        }

        private Dictionary<string, string> GetChanges()
        {
            lock (_gate)
            {
                var changes = _changedWallets;
                _changedWallets = new Dictionary<string, string>();
                return changes;
            }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer.Stop();
        }
    }
}