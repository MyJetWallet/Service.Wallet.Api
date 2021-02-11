using System;

namespace Service.Wallet.Api.Domain.Models
{
    public static class OrderIdGenerator
    {
        public static string Generate()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}