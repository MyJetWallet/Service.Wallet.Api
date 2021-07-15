using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.WalletApi;
using MyJetWallet.Sdk.WalletApi.Contracts;
using MyJetWallet.Sdk.WalletApi.Wallets;
using Service.BaseCurrencyConverter.Domain.Models;
using Service.BaseCurrencyConverter.Grpc;
using Service.BaseCurrencyConverter.Grpc.Models;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly IBaseCurrencyConverterService _baseCurrencyConverterService;
        private readonly IWalletService _walletService;

        public WalletController(IBaseCurrencyConverterService baseCurrencyConverterService, IWalletService walletService)
        {
            _baseCurrencyConverterService = baseCurrencyConverterService;
            _walletService = walletService;
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