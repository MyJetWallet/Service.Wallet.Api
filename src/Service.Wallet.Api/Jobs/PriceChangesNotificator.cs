using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.Prices;
using MyServiceBus.TcpContracts;
using Service.TradeHistory.Domain.Models;
using Service.Wallet.Api.Hubs;

namespace Service.Wallet.Api.Jobs
{
    public class PriceChangesNotificator : IStartable, IDisposable
    {
        public const int TimeoutMs = 500;

        private readonly IHubManager _hubManager;
        private readonly ILogger<PriceChangesNotificator> _logger;

        private Dictionary<string, BidAsk> _lastPrices = new Dictionary<string, BidAsk>();
        private readonly object _gate = new object();

        private readonly CancellationTokenSource _token = new CancellationTokenSource();
        private Task _process;

        public PriceChangesNotificator(IHubManager hubManager, ISubscriber<BidAsk> priceSubscriber, ILogger<PriceChangesNotificator> logger)
        {
            _hubManager = hubManager;
            _logger = logger;
            priceSubscriber.Subscribe(HandlePriceUpdate);
        }

        private ValueTask HandlePriceUpdate(BidAsk price)
        {
            lock (_gate) _lastPrices[price.LiquidityProvider + price.Id] = price;
            return new ValueTask();
        }

        public void Start()
        {
            _process = Task.Run(DoProcess, _token.Token);
        }

        private async Task DoProcess()
        {
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    Dictionary<string, BidAsk> last;
                    lock (_gate)
                    {
                        last = _lastPrices;
                        _lastPrices = new Dictionary<string, BidAsk>();
                    }

                    await _hubManager.ExecForeachConnection(ctx => ctx.SendPrices(last.Values));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected exception");
                }

                await Task.Delay(TimeoutMs);
            }
        }

        public void Dispose()
        {
            _token.Cancel();
            _process.Wait();
        }
    }
}