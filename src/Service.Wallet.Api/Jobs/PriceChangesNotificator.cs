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
        public int TimerDelayMs = 50;

        private readonly IHubManager _hubManager;
        
        private Dictionary<string, BidAsk> _lastPrices = new Dictionary<string, BidAsk>();
        private readonly object _gate = new object();

        private readonly MyTaskTimer _timer;

        public PriceChangesNotificator(IHubManager hubManager, ISubscriber<IReadOnlyList<BidAsk>> priceSubscriber, ILogger<PriceChangesNotificator> logger)
        {
            _hubManager = hubManager;
            priceSubscriber.Subscribe(HandlePriceUpdate);
            _timer = new MyTaskTimer(nameof(PriceChangesNotificator), TimeSpan.FromMilliseconds(TimerDelayMs), logger, DoProcess); 
        }

        private ValueTask HandlePriceUpdate(IReadOnlyList<BidAsk> prices)
        {
            lock (_gate)
            {
                foreach (var group in prices.GroupBy(e => $"{e.LiquidityProvider}{e.Id}"))
                {
                    var ts = group.Max(e => e.DateTime);
                    var price = group.First(e => e.DateTime == ts);
                    _lastPrices[group.Key] = price;
                }
            }

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
            { "LTCEUR", 180m },
            { "Index", 1m },
            { "ADAUSD", 1.2480m },
            { "XRPUSD", 0.46m },
            { "BATUSD", 0.9930m },
            { "DOTUSD", 35.17m }
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

            foreach (var symbol in _prices.Keys.Where(e => e != "Index").ToList())
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

            {
                var symbol = "Index";

                var price = _prices["Index"] = _prices["Index"] + 1;
                if (price > 1000000)
                    _prices["Index"] = 1;


                last[symbol] = new BidAsk()
                {
                    Ask = (double)price,
                    Bid = (double)price,
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