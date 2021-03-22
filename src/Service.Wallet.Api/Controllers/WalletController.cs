using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<Response<WalletBalancesMessage>> GetBalances([FromQuery, Required(AllowEmptyStrings = false, ErrorMessage = "walletId is required")] string walletId)
        {
            if (string.IsNullOrEmpty(walletId)) throw new WalletApiBadRequestException("walletId is required");

            var wallet = await HttpContext.GetWalletIdentityAsync(walletId);

            var data = await _balanceService.GetWalletBalancesAsync(new GetWalletBalancesRequest()
            {
                WalletId = walletId
            });

            return new Response<WalletBalancesMessage>(new WalletBalancesMessage()
            {
                Balances = data.Balances ?? new List<WalletBalance>()
            });
        }
    }
}