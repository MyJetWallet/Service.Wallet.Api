using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Authorization.Client.Http;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
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

        public WalletController(IWalletBalanceService balanceService)
        {
            _balanceService = balanceService;
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
    }
}