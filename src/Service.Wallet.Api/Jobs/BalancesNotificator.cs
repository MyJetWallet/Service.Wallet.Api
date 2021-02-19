using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.Balances.Domain.Models;
using Service.Wallet.Api.Hubs;

namespace Service.Wallet.Api.Jobs
{
    public class BalancesNotificator : IStartable, IDisposable
    {
        private readonly IHubManager _hubManager;
        private readonly IMyNoSqlServerDataReader<WalletBalanceNoSqlEntity> _reader;
        private readonly ILogger<BalancesNotificator> _logger;

        private Dictionary<string, string> _changedWallets = new Dictionary<string, string>();
        private object _gate = new object();

        private readonly CancellationTokenSource _token = new CancellationTokenSource();
        private Task _process;

        public BalancesNotificator(IHubManager hubManager, IMyNoSqlServerDataReader<WalletBalanceNoSqlEntity> reader,
            ILogger<BalancesNotificator> logger)
        {
            _hubManager = hubManager;
            _reader = reader;
            _logger = logger;
            _reader.SubscribeToUpdateEvents(HandleUpdate, HandleDelete);
        }

        private void HandleDelete(IReadOnlyList<WalletBalanceNoSqlEntity> entities)
        {
            throw new NotImplementedException();
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
            _process = Task.Run(DoProcess, _token.Token);
        }

        public void Dispose()
        {
            _token.Cancel();
            _process.Wait();
        }

        private async Task DoProcess()
        {
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendNotifications();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Exception at BalanceNotificator.DoProcess");
                }

                await Task.Delay(500);
                
            }
        }

        private async Task CheckAndSendNotifications()
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
                _logger.LogInformation("Balance updates. Count: {count}, Time: {ElapsedMilliseconds} ms",
                    countSent, sw.ElapsedMilliseconds);
            }
        }
    }
}