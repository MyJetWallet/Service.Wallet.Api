using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Authorization.Client.Http;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.BaseCurrencyConverter.Domain.Models;
using Service.BaseCurrencyConverter.Grpc;
using Service.BaseCurrencyConverter.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Contracts;
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

        public WalletController(IWalletBalanceService balanceService, IBaseCurrencyConverterService baseCurrencyConverterService)
        {
            _balanceService = balanceService;
            _baseCurrencyConverterService = baseCurrencyConverterService;
        }

        /// <summary>
        /// Get balances by walletId
        /// </summary>
        [HttpGet("wallet-balances")]
        public async Task<Response<WalletBalancesMessage>> GetBalances()
        {
            var wallet = this.GetWalletIdentity();

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
            var wallet = this.GetWalletIdentity();

            var data = await _baseCurrencyConverterService.GetConvertorMapToBaseCurrencyAsync(
                new GetConvertorMapToBaseCurrencyRequest()
                {
                    BaseAsset = baseAssetSymbol,
                    BrokerId = wallet.BrokerId
                });

            

            return new Response<BaseAssetConvertMap>(data);
        }
    }
}