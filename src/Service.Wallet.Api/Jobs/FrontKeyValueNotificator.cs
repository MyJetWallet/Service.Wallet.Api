using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.Abstractions;
using Service.FrontendKeyValue.Domain.Models.NoSql;
using Service.Wallet.Api.Hubs;

namespace Service.Wallet.Api.Jobs
{
    public class FrontKeyValueNotificator: IStartable, IDisposable
    {
        private readonly IMyNoSqlServerDataReader<FrontKeyValueNoSql> _reader;
        private readonly ILogger<FrontKeyValueNotificator> _logger;
        private readonly IHubManager _hubManager;
        private readonly Dictionary<string, string> _changedClients = new Dictionary<string, string>();

        private readonly MyTaskTimer _timer;

        public FrontKeyValueNotificator(IMyNoSqlServerDataReader<FrontKeyValueNoSql> reader, ILogger<FrontKeyValueNotificator> logger, IHubManager hubManager)
        {
            _logger = logger;
            _hubManager = hubManager;
            reader.SubscribeToUpdateEvents(Callback, Callback);
            _timer = new MyTaskTimer(nameof(FrontKeyValueNotificator), TimeSpan.FromMilliseconds(200), logger, DoTime).DisableTelemetry();
        }

        private async Task DoTime()
        {
            List<string> changedClients = null;

            lock (_changedClients)
            {
                if (_changedClients.Any())
                {
                    changedClients = _changedClients.Keys.ToList();
                    _changedClients.Clear();
                }
            }

            if (changedClients == null)
                return;

            var taskList = new List<Task>();

            foreach (var walletId in changedClients)
            {
                var contexts = _hubManager.TryGetContextByClientId(walletId);
                foreach (var context in contexts)
                {
                    taskList.Add(SendMessage(context, walletId));
                }
            }

            await Task.WhenAll(taskList);
        }

        private async Task SendMessage(HubClientConnection context, string walletId)
        {
            try
            {
                await context.SendKeyValuesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot send key value to {walletId}", walletId);
            }
        }

        private void Callback(IReadOnlyList<FrontKeyValueNoSql> changed)
        {
            lock (_changedClients)
            {
                foreach (var key in changed)
                {
                    _changedClients[key.PartitionKey] = key.PartitionKey;
                }
            }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Dispose()
        {
            _timer.Start();
            _timer.Dispose();
        }
    }
}