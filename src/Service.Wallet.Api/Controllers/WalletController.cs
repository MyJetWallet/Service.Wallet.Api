using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Sdk.Service;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.BaseCurrencyConverter.Domain.Models;
using Service.BaseCurrencyConverter.Grpc;
using Service.BaseCurrencyConverter.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Contracts;
using Service.Wallet.Api.Domain.Wallets;
using Service.Wallet.Api.Hubs.Dto;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletBalanceService _balanceService;
        private readonly IBaseCurrencyConverterService _baseCurrencyConverterService;
        private readonly IWalletService _walletService;

        public WalletController(IWalletBalanceService balanceService, IBaseCurrencyConverterService baseCurrencyConverterService, IWalletService walletService)
        {
            _balanceService = balanceService;
            _baseCurrencyConverterService = baseCurrencyConverterService;
            _walletService = walletService;
        }

        /// <summary>
        /// Get balances by walletId
        /// </summary>
        [HttpGet("wallet-balances")]
        public async Task<Response<WalletBalancesMessage>> GetBalances()
        {
            var clientId = this.GetClientIdentity();
            var wallet = await _walletService.GetDefaultWalletAsync(clientId);
            
            var data = await _balanceService.GetWalletBalancesAsync(new GetWalletBalancesRequest()
            {
                WalletId = wallet.WalletId
            });

            return new Response<WalletBalancesMessage>(new WalletBalancesMessage()
            {
                Balances = data.Balances ?? new List<WalletBalance>()
            });
        }

        /// <summary>
        /// Get balances by walletId
        /// </summary>
        [HttpGet("base-currency-converter-map/{baseAssetSymbol}")]
        public async Task<Response<BaseAssetConvertMap>> GetBaseCurrencyConverterMapAsync([FromRoute] string baseAssetSymbol)
        {
            baseAssetSymbol.AddToActivityAsTag("baseAssetSymbol");

            var clientId = this.GetClientIdentity();
            var wallet = await _walletService.GetDefaultWalletAsync(clientId);

            var data = await _baseCurrencyConverterService.GetConvertorMapToBaseCurrencyAsync(
                new GetConvertorMapToBaseCurrencyRequest()
                {
                    BaseAsset = baseAssetSymbol,
                    BrokerId = clientId.BrokerId
                });

            

            return new Response<BaseAssetConvertMap>(data);
        }
    }
}