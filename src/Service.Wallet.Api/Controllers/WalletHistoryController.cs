using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Domain.Orders;
using Service.TradeHistory.Domain.Models;
using Service.TradeHistory.Grpc;
using Service.TradeHistory.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/wallet-history")]
    public class WalletHistoryController: ControllerBase
    {
        private readonly IWalletTradeService _walletTradeService;
        //todo: get order by id
        //todo: get trade by id

        public WalletHistoryController(IWalletTradeService walletTradeService)
        {
            _walletTradeService = walletTradeService;
        }

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
        public async Task<Response<List<WalletTrade>>> GetTradeHistory([FromRoute] string wallet, [FromQuery] string instrumentSymbol, [FromQuery] int? take, [FromQuery] long? lastSequenceId)
        {
            var walletId = await HttpContext.GetWalletIdentityAsync(wallet);

            var data = await _walletTradeService.GetTradesAsync(new GetTradesRequest()
            {
                WalletId = walletId.WalletId,
                Take = take,
                LastSequenceId = lastSequenceId,
                Symbol = instrumentSymbol
            });
            
            return new Response<List<WalletTrade>>(data.Trades);
        }
    }
}