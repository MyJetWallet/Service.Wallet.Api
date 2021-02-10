using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.AssetsDictionary.Domain.Models;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Controllers.Models;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/trading")]
    public class TradingController: ControllerBase
    {
        //todo: нужен метод установки лимит ордера на обмен конкретного количества валюты
        //todo: в заявке надо сделать необходимым помимо волума указывать валюту волума, чтоб мы могли фиксировать котируемый или базовый обьем

        /// <summary>
        /// Create limit order on the wallet
        /// </summary>
        [HttpPost("create-limit-order")]
        public async Task<Response<CreatedOrderResponse>> CreateLimitOrderAsync([FromBody] CreateLimitOrderRequest request)
        {
            if (request == null) throw new WalletApiBadRequestException("request cannot be null");

            var walletId = await this.GetWalletIdentityAsync(request.WalletId);

            //todo: exec create order for walletId

            var response = new CreatedOrderResponse()
            {
                Type = OrderType.Limit,
                OrderId = Guid.NewGuid().ToString("N"),
                OrderPrice = request.Price
            };

            return new Response<CreatedOrderResponse>(response);
        }
        
        /// <summary>
        /// Execute fill-or-kill market order
        /// </summary>
        [HttpPost("create-market-order")]
        public async Task<Response<CreatedOrderResponse>> CreateMarketOrderAsync([FromBody] CreateMarketOrderRequest request)
        {
            if (request == null) throw new WalletApiBadRequestException("request cannot be null");

            var walletId = await this.GetWalletIdentityAsync(request.WalletId);

            //todo: exec create order for walletId

            var response = new CreatedOrderResponse()
            {
                Type = OrderType.Limit,
                OrderId = Guid.NewGuid().ToString("N"),
                OrderPrice = 100.45m
            };

            return new Response<CreatedOrderResponse>(response);
        }

        /// <summary>
        /// Cancel limit order by ID
        /// </summary>
        [HttpPost("cancel-order")]
        public async Task<Response> CancelOrderAsync(CancelOrderRequest request)
        {
            if (request == null) throw new WalletApiBadRequestException("request cannot be null");

            var walletId = await this.GetWalletIdentityAsync(request.WalletId);

            //todo: cancel order

            return Contracts.Response.OK();
        }

        /// <summary>
        /// Get list of Placed limit orders
        /// </summary>
        [HttpGet("order-list/{wallet}")]
        public async Task<Response<List<LimitOrder>>> GetActiveOrdersAsync([FromRoute] string wallet)
        {
            if (wallet == null) throw new WalletApiBadRequestException("request cannot be null");
            var walletId = await this.GetWalletIdentityAsync(wallet);

            //todo: get order from cache if not exist from service

            var response = new List<LimitOrder>()
            {
                new LimitOrder()
                {
                    WalletId = walletId.WalletId,
                    OrderId = OrderIdGenerator.Generate(),
                    Price = 200.34m,
                    Direction = Direction.Buy,
                    Volume = 0.45m,
                    FilledVolume = 0.1m,
                    InstrumentSymbol = "BTCUSD",
                    CreatedTime = DateTime.UtcNow.AddHours(-1),
                    LastUpdate = DateTime.UtcNow.AddMinutes(-10),
                    Status = OrderStatus.Placed,
                    Type = OrderType.Limit
                },

                new LimitOrder()
                {
                    WalletId = walletId.WalletId,
                    OrderId = OrderIdGenerator.Generate(),
                    Price = 200.34m,
                    Direction = Direction.Sell,
                    Volume = 2m,
                    FilledVolume = 0m,
                    InstrumentSymbol = "ETHUSD",
                    CreatedTime = DateTime.UtcNow.AddHours(-1),
                    LastUpdate = DateTime.UtcNow.AddMinutes(-10),
                    Status = OrderStatus.Placed,
                    Type = OrderType.Limit
                }
            };

            return new Response<List<LimitOrder>>(response);
        }
    }

    



    
}