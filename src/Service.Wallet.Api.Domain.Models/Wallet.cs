namespace Service.Wallet.Api.Domain.Models
{
    public class ClientWallet
    {
        public string WalletId { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Last activated wallet keeps as default wallet
        /// </summary>
        public bool IsDefault { get; set; }
    }
}