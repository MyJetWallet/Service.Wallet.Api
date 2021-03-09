namespace Service.Wallet.Api.Hubs
{
    public class HubNames
    {
        /// <summary>
        /// Welcome message
        /// </summary>
        public const string Welcome = "welcome";

        /// <summary>
        /// Pong message
        /// </summary>
        public const string Pong = "pong";

        /// <summary>
        /// [incoming] Ping message
        /// </summary>
        public const string Ping = "Ping";

        /// <summary>
        /// [incoming] Init the connection
        /// </summary>
        public const string Init = "Init";

        /// <summary>
        /// [incoming] Set wallet to the connection
        /// </summary>
        public const string SetWallet = "SetWallet";

        /// <summary>
        /// List of available wallets
        /// </summary>
        public const string WalletList = "wallet-list";

        /// <summary>
        /// List of available assets
        /// </summary>
        public const string AssetList = "asset-list";

        /// <summary>
        /// List of available spot instruments
        /// </summary>
        public const string SpotInstrumentList = "spot-insrument-list";

        /// <summary>
        /// Price by spot instrument
        /// </summary>
        public const string BidAsk = "spot-bidask";

        /// <summary>
        /// Balances by spot wallet
        /// </summary>
        public const string WalletBalances = "spot-wallet-balances";

        /// <summary>
        /// Active orders by spot wallet
        /// </summary>
        public const string ActiveOrders = "spot-active-orders";
        
        /// <summary>
        /// Trades by spot wallet
        /// </summary>
        public const string Trades = "spot-trades";

        /// <summary>
        /// [incoming] Init the connection
        /// </summary>
        public const string SetOrderBook = "SetOrderBook";
    }
}