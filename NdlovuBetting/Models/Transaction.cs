using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NdlovuBetting.Models
{
    // Credit = money in (deposit), Debit = money out (withdraw).
    public enum TransactionType
    {
        Debit = 0,
        Credit = 1
    }

    // One movement of demo money on a wallet account.
    public class Transaction
    {
        public int Id { get; set; }

        // The amount can never be zero.
        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; } = TransactionType.Credit;

        [MaxLength(250)]
        public string? Description { get; set; }

        // The date the transaction applies to (never in the future).
        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        // When the system recorded it - set automatically, never edited by the user.
        public DateTime CapturedAt { get; set; }

        // Which wallet this belongs to.
        public int AccountId { get; set; }
        public Account? Account { get; set; }

        // A credit adds to the balance; a debit subtracts. Used for the maths.
        public decimal SignedAmount => Type == TransactionType.Credit ? Amount : -Amount;
    }
}