using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Domain.Assets;
using MyJetWallet.Domain.Orders;
using Service.AssetsDictionary.Client;
using Service.BalanceHistory.Grpc;
using Service.BalanceHistory.Grpc.Models;
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

        private readonly IWalletBalanceUpdateService _balanceUpdateService;

        private readonly IAssetsDictionaryClient _assetsDictionaryClient;
        //todo: get order by id
        //todo: get trade by id

        public WalletHistoryController(IWalletTradeService walletTradeService, IWalletBalanceUpdateService balanceUpdateService, IAssetsDictionaryClient assetsDictionaryClient)
        {
            _walletTradeService = walletTradeService;
            _balanceUpdateService = balanceUpdateService;
            _assetsDictionaryClient = assetsDictionaryClient;
        }

        [HttpGet("{wallet}/balance-history")]
        public async Task<Response<List<BalanceHistoryItem>>> GetBalanceHistoryAsync([FromRoute] string wallet, [FromQuery][CanBeNull] int? take, [FromQuery] [CanBeNull] long? lastSequenceId, 
            [FromQuery] [CanBeNull] string assetSymbol)
        {
            var walletId = await HttpContext.GetWalletIdentityAsync(wallet);

            var data = await _balanceUpdateService.GetBalanceUpdatesAsync(new GetBalanceUpdateRequest()
            {
                Take = take,
                LastSequenceId = lastSequenceId,
                Symbol = assetSymbol,
                WalletId = walletId.WalletId,
                OnlyBalanceChanged = true
            });

            var response = data.BalanceUpdates
                .Select(e =>
                {
                    var amount = CalculateAmount(e.NewBalance, e.OldBalance, e.Symbol, walletId.BrokerId);
                    return new BalanceHistoryItem()
                    {
                        AssetSymbol = e.Symbol,
                        Type = MapEventType(e.EventType, amount > 0),
                        Amount = amount,
                        Timestamp = e.Timestamp,
                        SequenceId = e.SequenceId
                    };
                })
                .ToList();

            return new Response<List<BalanceHistoryItem>>(response);
        }

        private double CalculateAmount(double newBalance, double oldBalance, string symbol, string brokerId)
        {
            var asset = _assetsDictionaryClient.GetAssetById(new AssetIdentity()
            {
                BrokerId = brokerId,
                Symbol = symbol
            });


            if (asset == null)
            {
                return Math.Round(newBalance - oldBalance, 8);
            }

            return Math.Round(newBalance - oldBalance, asset.Accuracy);
        }

        private BalanceHistoryType MapEventType(string eventType, bool isPositive)
        {
            if (eventType == "CASH_IN_OUT_OPERATION" && isPositive)
                return BalanceHistoryType.Deposit;

            if (eventType == "CASH_IN_OUT_OPERATION" && !isPositive)
                return BalanceHistoryType.Withdrawal;

            return BalanceHistoryType.Trade;
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