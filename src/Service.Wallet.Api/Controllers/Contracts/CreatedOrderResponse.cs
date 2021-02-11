﻿using Service.Wallet.Api.Domain.Models;

namespace Service.Wallet.Api.Controllers.Contracts
{
    public class CreatedOrderResponse
    {
        public OrderType Type { get; set; }

        public string OrderId { get; set; }

        public decimal OrderPrice { get; set; }
    }
}