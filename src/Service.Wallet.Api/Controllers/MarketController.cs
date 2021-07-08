using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Wallet.Api.Controllers.Contracts;

namespace Service.Wallet.Api.Controllers
{    
    [ApiController]
    [Authorize]
    [Route("/api/v1/market")]
    public class MarketController
    {
        [HttpGet("reference")]
        public async Task<Response<List<MarketReferenceResponse>>> GetMarketReference()
        {
            var response = new List<MarketReferenceResponse>()
            {
                new MarketReferenceResponse()
                {
                    Id = "BTC",
                    Name = "Bitcoin",
                    AssociateAsset = "BTC",
                    AssociateAssetPair = "BTCUSD",
                    BrokerId = "myjetwallet",
                    IconUrl = "",
                    Weight = 100
                },
                new MarketReferenceResponse()
                {
                    Id = "BTCEur",
                    Name = "Bitcoin",
                    AssociateAsset = "BTC",
                    AssociateAssetPair = "BTCEUR",
                    BrokerId = "myjetwallet",
                    IconUrl = "",
                    Weight = 90
                },
                new MarketReferenceResponse()
                {
                    Id = "ETH",
                    Name = "Ethereum",
                    AssociateAsset = "ETH",
                    AssociateAssetPair = "ETHUSD",
                    BrokerId = "myjetwallet",
                    IconUrl = "",
                    Weight = 100
                },
                new MarketReferenceResponse()
                {
                    Id = "ETHtoBTC",
                    Name = "Ethereum",
                    AssociateAsset = "ETH",
                    AssociateAssetPair = "BTCETH",
                    BrokerId = "myjetwallet",
                    IconUrl = "",
                    Weight = 80
                }
            };
            return new Response<List<MarketReferenceResponse>>(response);
        }
    }
}