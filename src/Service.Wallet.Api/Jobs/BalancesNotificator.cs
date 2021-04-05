using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.Abstractions;
using Service.Balances.Domain.Models;
using Service.Wallet.Api.Hubs;

namespace Service.Wallet.Api.Jobs
{
    public class BalancesNotificator : IStartable, IDisposable
    {
        private readonly IHubManager _hubManager;
        private readonly ILogger<BalancesNotificator> _logger;

        private Dictionary<string, string> _changedWallets = new Dictionary<string, string>();
        private readonly object _gate = new object();

        private readonly MyTaskTimer _timer;

        public BalancesNotificator(IHubManager hubManager, IMyNoSqlServerDataReader<WalletBalanceNoSqlEntity> reader,
            ILogger<BalancesNotificator> logger)
        {
            _hubManager = hubManager;
            _logger = logger;
            reader.SubscribeToUpdateEvents(HandleUpdate, HandleDelete);
            _timer = MyTaskTimer.Create<BalancesNotificator>(TimeSpan.FromMilliseconds(500), logger, DoProcess).DisableTelemetry();
        }

        private void HandleDelete(IReadOnlyList<WalletBalanceNoSqlEntity> entities)
        {
        }

        private void HandleUpdate(IReadOnlyList<WalletBalanceNoSqlEntity> entities)
        {
            lock (_gate)
            {
                foreach (var entity in entities)
                {
                    _changedWallets[entity.PartitionKey] = entity.PartitionKey;
                }
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
            Stop();
        }

        private async Task DoProcess()
        {
            var sw = new Stopwatch();
            sw.Start();

            Dictionary<string, string> changes;
            lock (_gate)
            {
                if (!_changedWallets.Any())
                    return;

                changes = _changedWallets;
                _changedWallets = new Dictionary<string, string>();
            }

            var countSent = 0;
            foreach (var walletId in changes.Keys)
            {
                var contexts = _hubManager.TryGetContextByWalletId(walletId);
                foreach (var context in contexts)
                {
                    try
                    {
                        countSent++;
                        await context.SendWalletBalancesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Cannot send wallet balances to {walletId}", walletId);
                    }
                }
            }

            sw.Stop();
            if (countSent > 0)
            {
                _logger.LogDebug("Balance updates. Count: {count}, Time: {ElapsedMilliseconds} ms",
                    countSent, sw.ElapsedMilliseconds);
            }
        }
    }
}