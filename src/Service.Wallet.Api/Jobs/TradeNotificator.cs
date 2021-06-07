using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.BalanceHistory.ServiceBus;
using Service.Wallet.Api.Hubs;
using Service.Wallet.Api.Hubs.Dto;

namespace Service.Wallet.Api.Jobs
{
    public class TradeNotificator : IDisposable
    {
        private readonly IHubManager _hubManager;
        private readonly ILogger<TradeNotificator> _logger;
        private readonly MyTaskTimer _timer;
        private readonly MyBuffer<WalletTradeMessage> _buffer = new MyBuffer<WalletTradeMessage>();


        public TradeNotificator(IHubManager hubManager, ISubscriber<IReadOnlyList<WalletTradeMessage>> tradeSubscriber, ILogger<TradeNotificator> logger)
        {
            _hubManager = hubManager;
            _logger = logger;
            _timer = new MyTaskTimer(nameof(TradeNotificator), TimeSpan.FromMilliseconds(500), logger, DoSendNotifications).DisableTelemetry();
            tradeSubscriber.Subscribe(HandleEvent);
        }

        private ValueTask HandleEvent(IReadOnlyList<WalletTradeMessage> trades)
        {
            _buffer.AddRange(trades);
            return ValueTask.CompletedTask;
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

        private ValueTask HandleEvent(WalletTradeMessage trade)
        {
            _buffer.Add(trade);
            return ValueTask.CompletedTask;
        }

        private async Task DoSendNotifications()
        {
            var sw = new Stopwatch();
            sw.Start();
            
            var buf = _buffer.ExtractAll();

            var data = buf.GroupBy(e => e.WalletId);

            var tasks = new List<Task>();

            foreach (var trades in data)
            {
                var contextList = _hubManager.TryGetContextByWalletId(trades.Key);

                var message = new TradesMessage()
                {
                    WalletId = trades.Key,
                    Trades = trades.Select(e => e.Trade).ToList()
                };

                tasks.AddRange(contextList.Select(ctx => ctx.SendTradesAsync(message)));
            }

            await Task.WhenAll(tasks);

            if (tasks.Any())
            {
                _logger.LogDebug("Trades sent. Count wallet: {count}, Time: {ElapsedMilliseconds} ms",
                    tasks.Count, sw.ElapsedMilliseconds);
            }
        }
    }
}