using System;
using System.Globalization;
using System.Threading.Tasks;
using ME.Contracts.Api.IncomingMessages;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Assets;
using MyJetWallet.Domain.Orders;
using MyJetWallet.MatchingEngine.Grpc.Api;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;
using Service.Service.KYC.Client;
using Service.Service.KYC.Domain.Models;
using Service.Service.KYC.Grpc.Models;
using Service.Wallet.Api.Domain.Contracts;

namespace Service.Wallet.Api.Domain.Orders
{
    public interface IOrderService
    {
        ValueTask<string> CreateLimitOrderAsync(IJetWalletIdentity walletId, string symbol, double price, double volume,
            OrderSide side);

        Task<(string, double)> CreateMarketOrderAsync(IJetWalletIdentity walletId, string symbol, double volume,
            OrderSide side);

        Task CancelOrderAsync(IJetWalletIdentity walletId, string orderId);
    }

    public class OrderService : IOrderService
    {
        private readonly ITradingServiceClient _tradingServiceClient;
        private readonly ISpotInstrumentDictionaryClient _spotInstrumentDictionaryClient;
        private readonly IKycStatusClient _kycStatusClient;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ITradingServiceClient tradingServiceClient,
            ISpotInstrumentDictionaryClient spotInstrumentDictionaryClient, 
            IKycStatusClient kycStatusClient,
            ILogger<OrderService> logger)
        {
            _tradingServiceClient = tradingServiceClient;
            _spotInstrumentDictionaryClient = spotInstrumentDictionaryClient;
            _kycStatusClient = kycStatusClient;
            _logger = logger;
        }


        public async ValueTask<string> CreateLimitOrderAsync(IJetWalletIdentity walletId, string symbol, double price,
            double volume,
            OrderSide side)
        {
            //todo: add order parameter logical validations
            var spotInstrument = _spotInstrumentDictionaryClient.GetSpotInstrumentById(new SpotInstrumentIdentity()
            {
                BrokerId = walletId.BrokerId,
                Symbol = symbol
            });
            
            ValidateInstrument(spotInstrument);
            ValidateKyc(spotInstrument, walletId.BrokerId, walletId.ClientId);

            var volumeSign = side == OrderSide.Buy ? volume : -volume;

            var order = new LimitOrder()
            {
                BrokerId = walletId.BrokerId,
                AccountId = walletId.ClientId,
                WalletId = walletId.WalletId,

                Id = Guid.NewGuid().ToString("N"),
                MessageId = Guid.NewGuid().ToString("N"),

                Type = LimitOrder.Types.LimitOrderType.Limit,
                AssetPairId = symbol,
                Price = price.ToString(CultureInfo.InvariantCulture),
                Volume = volumeSign.ToString(CultureInfo.InvariantCulture),
                WalletVersion = -1
            };

            var resp = await _tradingServiceClient.LimitOrderAsync(order);

            CheckMeResponse(resp.Status, resp.StatusReason);

            return order.Id;
        }

        public async Task<(string, double)> CreateMarketOrderAsync(IJetWalletIdentity walletId, string symbol,
            double volume, OrderSide side)
        {
            //todo: add order parameter logical validations
            var spotInstrument = _spotInstrumentDictionaryClient.GetSpotInstrumentById(new SpotInstrumentIdentity()
            {
                BrokerId = walletId.BrokerId,
                Symbol = symbol
            });
            
            ValidateInstrument(spotInstrument);
            ValidateKyc(spotInstrument, walletId.BrokerId, walletId.ClientId);

            var volumeSign = side == OrderSide.Buy ? volume : -volume;

            var order = new MarketOrder()
            {
                BrokerId = walletId.BrokerId,
                AccountId = walletId.ClientId,
                WalletId = walletId.WalletId,

                Id = Guid.NewGuid().ToString("N"),
                MessageId = Guid.NewGuid().ToString("N"),

                AssetPairId = symbol,
                Volume = volumeSign.ToString(CultureInfo.InvariantCulture),
                WalletVersion = -1,
                
                Straight = true
            };

            var resp = await _tradingServiceClient.MarketOrderAsync(order);

            CheckMeResponse(resp.Status, resp.StatusReason);

            return (order.Id, double.Parse(resp.Price));
        }

        private void CheckMeResponse(Status respStatus, string respStatusReason)
        {
            if (respStatus == Status.LimitOrderNotFound)
                return;

            if (respStatus == Status.LowBalance || respStatus == Status.NotEnoughFunds)
                RejectOrder(ApiResponseCodes.LowBalance, respStatus, respStatusReason);

            else if (respStatus == Status.NoLiquidity)
                RejectOrder(ApiResponseCodes.NotEnoughLiquidityForMarketOrder, respStatus, respStatusReason);

            else if (respStatus == Status.InvalidOrderValue)
                RejectOrder(ApiResponseCodes.InvalidOrderValue, respStatus, respStatusReason);

            else if (respStatus != Status.Ok)
                RejectOrder(ApiResponseCodes.InternalServerError, respStatus, respStatusReason);
        }

        public async Task CancelOrderAsync(IJetWalletIdentity walletId, string orderId)
        {
            var resp = await _tradingServiceClient.CancelLimitOrderAsync(new LimitOrderCancel()
            {
                BrokerId = walletId.BrokerId,
                AccountId = walletId.ClientId,
                WalletId = walletId.WalletId,

                Id = Guid.NewGuid().ToString("N"),
                MessageId = Guid.NewGuid().ToString("N"),

                LimitOrderId = {orderId},
                WalletVersion = -1
            });

            CheckMeResponse(resp.Status, resp.StatusReason);
        }

        private void ValidateInstrument(ISpotInstrument spotInstrument)
        {
            if (spotInstrument == null)
            {
                RejectOrder(ApiResponseCodes.InvalidInstrument, null, "Unknown instrument.");
                return;
            }

            if (!spotInstrument.IsEnabled)
            {
                RejectOrder(ApiResponseCodes.InvalidInstrument, null, "Disabled instrument.");
            }
        }
        
        private void ValidateKyc(ISpotInstrument spotInstrument, string brokerId, string clientId)
        {
            if (!spotInstrument.KycRequiredForTrade) return;
            var kycStatus = _kycStatusClient.GetClientKycStatus(new KycStatusRequest()
            {
                BrokerId = brokerId,
                ClientId = clientId
            });

            if (kycStatus != null && kycStatus.Status != KycStatus.Verified)
            {
                RejectOrder(ApiResponseCodes.KycNotPassed, null, "KYC not passed.");
            }
        }
        
        private void RejectOrder(ApiResponseCodes code, Status? status, string statusReason)
        {
            throw new WalletApiErrorException($"Cannot register order in ME: {status?.ToString()} ({statusReason})",  code);
        }
    }
}