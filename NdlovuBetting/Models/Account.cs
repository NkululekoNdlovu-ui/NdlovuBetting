using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NdlovuBetting.Models
{
    // Whether the wallet is usable or closed.
    public enum AccountStatus
    {
        Open = 0,
        Closed = 1
    }

    // A wallet account belonging to one user. Holds the demo-money balance.
    public class Account
    {
        public int Id { get; set; }

        // Account numbers must be unique (e.g. WAL-000123).
        [Required]
        [MaxLength(20)]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = "";

        // Balance is controlled by the system only - never typed in directly.
        // The private setter stops forms from changing it; only our code can.
        [Precision(18, 2)]
        public decimal Balance { get; private set; }

        public AccountStatus Status { get; set; } = AccountStatus.Open;

        public DateTime CreatedAt { get; set; }

        // Which user owns this wallet.
        public int UserId { get; set; }
        public User? User { get; set; }

        // The deposits/withdrawals on this wallet.
        public List<Transaction> Transactions { get; set; } = new();

        // Only our service code calls this to update the balance.
        public void SetBalance(decimal value) => Balance = value;

        public bool IsClosed => Status == AccountStatus.Closed;
    }
}