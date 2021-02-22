using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Domain.Orders;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/wallet-history")]
    public class WalletHistoryController: ControllerBase
    {
        //todo: get order by id
        //todo: get trade by id

        [HttpGet("{wallet}/balance-history")]
        public async Task<Response<List<BalanceHistoryItem>>> GetBalanceHistoryAsync([FromRoute] string wallet, [FromQuery] int take, [FromQuery] DateTime? startTime, [FromQuery] string assetSymbol)
        {
            var walletId = await HttpContext.GetWalletIdentityAsync(wallet);


            var response = new List<BalanceHistoryItem>();

            if (!string.IsNullOrEmpty(assetSymbol))
            {
                response.Add(
                    new BalanceHistoryItem()
                    {
                        AssetSymbol = assetSymbol,
                        Type = BalanceHistoryType.Deposit,
                        Amount = 1000m,
                        Timestamp = DateTime.UtcNow
                    });

                response.Add(
                    new BalanceHistoryItem()
                    {
                        AssetSymbol = assetSymbol,
                        Type = BalanceHistoryType.Withdrawal,
                        Amount = 500m,
                        Timestamp = DateTime.UtcNow
                    });
            }
            else
            {
                response.Add(
                    new BalanceHistoryItem()
                    {
                        AssetSymbol = "USD",
                        Type = BalanceHistoryType.Deposit,
                        Amount = 1000m,
                        Timestamp = DateTime.UtcNow
                    });

                response.Add(
                    new BalanceHistoryItem()
                    {
                        AssetSymbol = "EUR",
                        Type = BalanceHistoryType.Withdrawal,
                        Amount = 500m,
                        Timestamp = DateTime.UtcNow
                    });
            }

            return new Response<List<BalanceHistoryItem>>(response);
        }

        [HttpGet("{wallet}/trade-history")]
        public async Task<Response<List<TradeHistory>>> GetTradeHistory([FromRoute] string wallet, [FromQuery] int take, [FromQuery] DateTime? startTime, [FromQuery] string instrumentSymbol)
        {
            var walletId = await HttpContext.GetWalletIdentityAsync(wallet);


            var response = new List<TradeHistory>()
            {
                new TradeHistory()
                {
                    TradeId = Guid.NewGuid().ToString("N"),
                    OrderId = OrderIdGenerator.Generate(),
                    Side = OrderSide.Buy,
                    Type = OrderType.Market,
                    Price = 300.33m,
                    WalletId = wallet,
                    InstrumentSymbol = "BTCUSD",
                    Timestamp = DateTime.UtcNow,
                    Volume = 0.001m,
                    QuoteVolume = 300.33m * 0.001m
                }
            };
            
            return new Response<List<TradeHistory>>(response);
        }
    }
}