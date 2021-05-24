using System;
using System.Threading.Tasks;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Orders;
using Service.Liquidity.Converter.Grpc.Models;

namespace Service.Wallet.Api.Domain.Swaps
{
    public interface ISwapService
    {
        Task<Quote> GetSwapQuoteAsync(
            IJetWalletIdentity walletId,
            string symbol,
            string assetSymbol,
            double volume,
            OrderSide side);

        Task<(bool, Quote)> ExecuteSwapQuoteAsync(
            IJetWalletIdentity walletId,
            string operationId);
    }
}