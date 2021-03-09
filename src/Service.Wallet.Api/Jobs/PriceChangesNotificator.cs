using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.Prices;
using MyJetWallet.Sdk.Service.Tools;
using Service.Wallet.Api.Hubs;

namespace Service.Wallet.Api.Jobs
{
    public class PriceChangesNotificator : IStartable, IDisposable
    {
        public int TimerDelayMs = 100;

        private readonly IHubManager _hubManager;
        
        private Dictionary<string, BidAsk> _lastPrices = new Dictionary<string, BidAsk>();
        private readonly object _gate = new object();

        private readonly MyTaskTimer _timer;

        public PriceChangesNotificator(IHubManager hubManager, ISubscriber<BidAsk> priceSubscriber, ILogger<PriceChangesNotificator> logger)
        {
            _hubManager = hubManager;
            priceSubscriber.Subscribe(HandlePriceUpdate);
            _timer = new MyTaskTimer(nameof(PriceChangesNotificator), TimeSpan.FromMilliseconds(TimerDelayMs), logger, DoProcess); 
        }

        private ValueTask HandlePriceUpdate(BidAsk price)
        {
            lock (_gate) _lastPrices[price.LiquidityProvider + price.Id] = price;
            return new ValueTask();
        }

        public void Start()
        {
            _timer.Start();
        }

        private async Task DoProcess()
        {
            Dictionary<string, BidAsk> last;
            lock (_gate)
            {
                last = _lastPrices;
                _lastPrices = new Dictionary<string, BidAsk>();
            }

            await _hubManager.ExecForeachConnection(ctx => ctx.SendPrices(last.Values));
        }

        public void Dispose()
        {
            _timer.Stop();
        }
    }
}