namespace NdlovuBetting.Models.ViewModels
{
    // One transaction row on the account details page.
    public class TransactionRow
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Type { get; set; } = "Credit";   // "Credit" or "Debit"
        public string? Description { get; set; }
        public decimal SignedAmount { get; set; }       // + for credit, - for debit
        public DateTime CapturedAt { get; set; }
    }

    // The full account details page.
    public class AccountDetailsViewModel
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = "";
        public decimal Balance { get; set; }
        public string Status { get; set; } = "Open";    // "Open" or "Closed"
        public bool IsClosed { get; set; }

        // Owner info (for the summary + back link)
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = "";

        public List<TransactionRow> Transactions { get; set; } = new();
    }
}