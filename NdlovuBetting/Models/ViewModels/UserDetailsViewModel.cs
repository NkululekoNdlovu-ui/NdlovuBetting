namespace NdlovuBetting.Models.ViewModels
{
    // One account row on the user details page.
    public class AccountRow
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = "";
        public decimal Balance { get; set; }
        public string Status { get; set; } = "Open";   // "Open" or "Closed"
    }

    // The full user details page: the person's info + their accounts.
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string Surname { get; set; } = "";
        public string FullName => $"{FirstName} {Surname}";
        public string IdNumber { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string Role { get; set; } = "Client";
        public DateTime CreatedAt { get; set; }

        public List<AccountRow> Accounts { get; set; } = new();
    }
}