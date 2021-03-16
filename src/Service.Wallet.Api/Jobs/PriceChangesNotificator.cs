using System;
using System.Collections.Generic;
using System.Linq;
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
        public int TimerDelayMs = 500;

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


        private Dictionary<string, decimal> _prices = new Dictionary<string, decimal>()
        {
            { "BTCUSD", 50000m },
            { "ETHUSD", 1800m },
            { "LTCUSD", 200m },
            { "BTCEUR", 49000m },
            { "ETHEUR", 1600m },
            { "LTCEUR", 180m }
        };

        private Random _rnd = new Random();

        private async Task DoProcess()
        {
            Dictionary<string, BidAsk> last;
            lock (_gate)
            {
                last = _lastPrices;
                _lastPrices = new Dictionary<string, BidAsk>();
            }

            var dt = DateTime.UtcNow;

            foreach (var symbol in _prices.Keys.ToList())
            {
                // 0.00001
                // 0.00050
                // 0.00100
                var d = _rnd.Next(100) / 10000m - 0.0050m;

                var price = _prices[symbol];

                price = price + price * d;
                price = Math.Round(price, 4);
                _prices[symbol] = price;

                last[symbol] = new BidAsk()
                {
                    Ask = (double) price,
                    Bid = (double) price,
                    Id = symbol,
                    LiquidityProvider = "jetwallet",
                    DateTime = dt
                };
            }

            await _hubManager.ExecForeachConnection(ctx => ctx.SendPrices(last.Values));
        }

        public void Dispose()
        {
            _timer.Stop();
        }
    }
}