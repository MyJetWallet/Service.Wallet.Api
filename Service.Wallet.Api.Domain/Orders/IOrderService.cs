using System;
using System.Globalization;
using System.Threading.Tasks;
using ME.Contracts.Api.IncomingMessages;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Orders;
using MyJetWallet.MatchingEngine.Grpc.Api;
using Service.Wallet.Api.Domain.Contracts;
using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Domain.Orders
{
    public interface IOrderService
    {
        ValueTask<string> CreateLimitOrderAsync(IJetWalletIdentity walletId, string symbol, double price, double volume, OrderSide side);
        Task<(string, double)> CreateMarketOrderAsync(IJetWalletIdentity walletId, string symbol, double volume, OrderSide side);
        Task CancelOrderAsync(IJetWalletIdentity walletId, string orderId);
    }

    public class OrderService : IOrderService
    {
        private readonly ITradingServiceClient _tradingServiceClient;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ITradingServiceClient tradingServiceClient, ILogger<OrderService> logger)
        {
            _tradingServiceClient = tradingServiceClient;
            _logger = logger;
        }


        public async ValueTask<string> CreateLimitOrderAsync(IJetWalletIdentity walletId, string symbol, double price, double volume,
            OrderSide side)
        {
            //todo: add order parameter logical validations

            var volumeSign = side == OrderSide.Buy ? volume : -volume;

            var order = new ME.Contracts.Api.IncomingMessages.LimitOrder()
            {
                BrokerId = walletId.BrokerId,
                AccountId = walletId.ClientId,
                WalletId = walletId.WalletId,

                Id = Guid.NewGuid().ToString("N"),
                MessageId = Guid.NewGuid().ToString("N"),

                Type = ME.Contracts.Api.IncomingMessages.LimitOrder.Types.LimitOrderType.Limit,
                AssetPairId = symbol,
                Price = price.ToString(CultureInfo.InvariantCulture),
                Volume = volumeSign.ToString(CultureInfo.InvariantCulture),
                WalletVersion = -1
            };

            var resp = await _tradingServiceClient.LimitOrderAsync(order);

            if (resp.Status != Status.Ok)
            {
                _logger.LogError("Cannot register order in ME: {statusText} ({statusReason})", resp.Status.ToString(), resp.StatusReason);
                throw new WalletApiErrorException(
                    $"Cannot register order in ME: {resp.Status.ToString()} ({resp.StatusReason})",
                    ApiResponseCodes.InternalServerError);
            }

            return order.Id;
        }

        public async Task<(string, double)> CreateMarketOrderAsync(IJetWalletIdentity walletId, string symbol, double volume, OrderSide side)
        {
            //todo: add order parameter logical validations

            var volumeSign = side == OrderSide.Buy ? volume : -volume;

            var order = new ME.Contracts.Api.IncomingMessages.MarketOrder()
            {
                BrokerId = walletId.BrokerId,
                AccountId = walletId.ClientId,
                WalletId = walletId.WalletId,

                Id = Guid.NewGuid().ToString("N"),
                MessageId = Guid.NewGuid().ToString("N"),

                AssetPairId = symbol,
                Volume = volumeSign.ToString(CultureInfo.InvariantCulture),
                WalletVersion = -1
            };

            var resp = await _tradingServiceClient.MarketOrderAsync(order);

            if (resp.Status != Status.Ok)
            {
                _logger.LogError("Cannot register order in ME: {statusText} ({statusReason})", resp.Status.ToString(), resp.StatusReason);
                throw new WalletApiErrorException(
                    $"Cannot register order in ME: {resp.Status.ToString()} ({resp.StatusReason})",
                    ApiResponseCodes.InternalServerError);
            }

            return (order.Id, double.Parse(resp.Price));
        }

        public async Task CancelOrderAsync(IJetWalletIdentity walletId, string orderId)
        {
            await _tradingServiceClient.CancelLimitOrderAsync(new LimitOrderCancel()
            {
                BrokerId = walletId.BrokerId,
                AccountId = walletId.ClientId,
                WalletId = walletId.WalletId,

                Id = Guid.NewGuid().ToString("N"),
                MessageId = Guid.NewGuid().ToString("N"),

                LimitOrderId = { orderId },
                WalletVersion = -1
            });
        }
    }
}