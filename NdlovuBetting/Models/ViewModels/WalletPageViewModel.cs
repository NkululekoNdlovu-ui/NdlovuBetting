using NdlovuBetting.Models.Dtos;

namespace NdlovuBetting.Models.ViewModels
{
    // Everything the dashboard + wallet popup + profile popup need on one page.
    public class WalletPageViewModel
    {
        // Profile
        public string FirstName { get; set; } = "";
        public string Surname { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string IdNumber { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        // Wallet
        public string AccountNumber { get; set; } = "";
        public decimal Balance { get; set; }
        public bool IsClosed { get; set; }

        // History (newest first)
        public List<Transaction> Transactions { get; set; } = new();

        // For the deposit/withdraw form
        public WalletActionDto Movement { get; set; } = new();

        // For the edit-profile form
        public ProfileDto Profile { get; set; } = new();
    }
}