using System.Collections.Generic;
using Service.AssetsDictionary.Domain.Models;

namespace Service.Wallet.Api.Domain.Models.Assets
{
    public class WalletSpotInstrument
    {
        public string Symbol { get; set; }
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public int Accuracy { get; set; }
        public decimal MinVolume { get; set; }
        public decimal MaxVolume { get; set; }
        public decimal MaxOppositeVolume { get; set; }

        public static WalletSpotInstrument Create(ISpotInstrument instrument)
        {
            return new WalletSpotInstrument()
            {
                Symbol = instrument.Symbol,
                Accuracy = instrument.Accuracy,
                BaseAsset = instrument.BaseAsset,
                QuoteAsset = instrument.QuoteAsset,
                MaxOppositeVolume = instrument.MaxOppositeVolume,
                MaxVolume = instrument.MaxVolume,
                MinVolume = instrument.MinVolume
            };
        }
    }
}