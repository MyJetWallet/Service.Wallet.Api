using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.Orders;
using MyJetWallet.MatchingEngine.Grpc.Api;
using Service.ActiveOrders.Grpc;
using Service.ActiveOrders.Grpc.Models;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Contracts;
using Service.Wallet.Api.Domain.Models;
using Service.Wallet.Api.Domain.Orders;

namespace Service.Wallet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/v1/trading")]
    public class TradingController: ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IActiveOrderService _activeOrderService;

        //todo: нужен метод установки лимит ордера на обмен конкретного количества валюты
        //todo: в заявке надо сделать необходимым помимо волума указывать валюту волума, чтоб мы могли фиксировать котируемый или базовый обьем

        public TradingController(IOrderService orderService, IActiveOrderService activeOrderService)
        {
            _orderService = orderService;
            _activeOrderService = activeOrderService;
        }

        /// <summary>
        /// Create limit order on the wallet
        /// </summary>
        [HttpPost("create-limit-order")]
        public async Task<Response<CreatedOrderResponse>> CreateLimitOrderAsync([FromBody] CreateLimitOrderRequest request)
        {
            if (request == null) throw new WalletApiBadRequestException("request cannot be null");
            if (request.Volume <= 0) throw new WalletApiBadRequestException("volume cannot be zero or negative");
            if (request.Price <= 0) throw new WalletApiBadRequestException("price cannot be zero or negative");
            if (string.IsNullOrEmpty(request.InstrumentSymbol)) throw new WalletApiBadRequestException("InstrumentSymbol cannot be empty");

            var walletId = await HttpContext.GetWalletIdentityAsync(request.WalletId);

            var orderId = await _orderService.CreateLimitOrderAsync(walletId, request.InstrumentSymbol, request.Price, request.Volume, request.Side);

            var response = new CreatedOrderResponse()
            {
                Type = OrderType.Limit,
                OrderId = orderId,
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
            if (request.Volume <= 0) throw new WalletApiBadRequestException("volume cannot be zero or negative");
            if (string.IsNullOrEmpty(request.InstrumentSymbol)) throw new WalletApiBadRequestException("InstrumentSymbol cannot be empty");

            var walletId = await HttpContext.GetWalletIdentityAsync(request.WalletId);

            //todo: exec create order for walletId

            (string orderId, double price) = await _orderService.CreateMarketOrderAsync(walletId, request.InstrumentSymbol, request.Volume, request.Side);

            var response = new CreatedOrderResponse()
            {
                Type = OrderType.Market,
                OrderId = orderId,
                OrderPrice = price
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

            var walletId = await HttpContext.GetWalletIdentityAsync(request.WalletId);

            //todo: cancel order

            await _orderService.CancelOrderAsync(walletId, request.OrderId);

            return Contracts.Response.OK();
        }

        /// <summary>
        /// Get list of Placed limit orders
        /// </summary>
        [HttpGet("order-list/{wallet}")]
        public async Task<Response<List<SpotOrder>>> GetActiveOrdersAsync([FromRoute] string wallet)
        {
            if (wallet == null) throw new WalletApiBadRequestException("request cannot be null");
            var walletId = await HttpContext.GetWalletIdentityAsync(wallet);

            var orders = await _activeOrderService.GetActiveOrdersAsync(new GetActiveOrdersRequest()
            {
                WalletId = walletId.WalletId
            });

            var response = orders.Orders.Select(e => new SpotOrder()
            {
                OrderId = e.OrderId,
                Price = e.Price,
                Side = e.Side,
                Volume = e.Volume,
                RemainingVolume = e.RemainingVolume,
                InstrumentSymbol = e.InstrumentSymbol,
                CreatedTime = e.CreatedTime,
                LastUpdate = e.LastUpdate,
                Status = e.Status,
                Type = e.Type
            }).ToList();

            return new Response<List<SpotOrder>>(response);
        }
    }
}