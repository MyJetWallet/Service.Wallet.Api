using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Domain.Assets;
using Service.AssetsDictionary.Client;
using Service.BalanceHistory.Domain.Models;
using Service.BalanceHistory.Grpc;
using Service.BalanceHistory.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Models;
using Service.Wallet.Api.Domain.Wallets;

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
        private readonly ISwapHistoryService _swapHistoryService;

        private readonly IWalletService _walletService;
        //todo: get order by id
        //todo: get trade by id

        public WalletHistoryController(IWalletTradeService walletTradeService, IWalletBalanceUpdateService balanceUpdateService, IAssetsDictionaryClient assetsDictionaryClient,
            IWalletService walletService, ISwapHistoryService swapHistoryService)
        {
            _walletTradeService = walletTradeService;
            _balanceUpdateService = balanceUpdateService;
            _assetsDictionaryClient = assetsDictionaryClient;
            _walletService = walletService;
            _swapHistoryService = swapHistoryService;
        }

        [HttpGet("balance-history")]
        public async Task<Response<List<BalanceHistoryItem>>> GetBalanceHistoryAsync([FromQuery][CanBeNull] int? take, [FromQuery] [CanBeNull] long? lastSequenceId, 
            [FromQuery] [CanBeNull] string assetSymbol)
        {
            var clientId = this.GetClientIdentity();
            var wallet = await _walletService.GetDefaultWalletAsync(clientId);

            var data = await _balanceUpdateService.GetBalanceUpdatesAsync(new GetBalanceUpdateRequest()
            {
                Take = take,
                LastSequenceId = lastSequenceId,
                Symbol = assetSymbol,
                WalletId = wallet.WalletId,
                OnlyBalanceChanged = true
            });

            var response = data.BalanceUpdates
                .Select(e =>
                {
                    var amount = CalculateAmount(e.NewBalance, e.OldBalance, e.Symbol, clientId.BrokerId);
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

        [HttpGet("trade-history")]
        public async Task<Response<List<WalletTrade>>> GetTradeHistory([FromQuery] string instrumentSymbol, [FromQuery] int? take, [FromQuery] long? lastSequenceId)
        {
            var clientId = this.GetClientIdentity();
            var wallet = await _walletService.GetDefaultWalletAsync(clientId);

            var data = await _walletTradeService.GetTradesAsync(new GetTradesRequest()
            {
                WalletId = wallet.WalletId,
                Take = take,
                LastSequenceId = lastSequenceId,
                Symbol = instrumentSymbol
            });
            
            return new Response<List<WalletTrade>>(data.Trades);
        }
        [HttpGet("swap-history")]
        public async Task<Response<List<Swap>>> GetSwapHistory([FromQuery] int batchSize, [FromQuery] DateTime? lastDate, [FromQuery] [CanBeNull] string walletId)
        {
            var request = new GetSwapsRequest() { BatchSize = batchSize};
            if (lastDate != null)
            {
                request.LastDate = (DateTime)lastDate;
            }

            if (!string.IsNullOrWhiteSpace(walletId))
            {
                request.WalletId = walletId;
            }

            var data = await _swapHistoryService.GetSwapsAsync(request);
            return new Response<List<Swap>>(data.SwapCollection);
        }
    }
}