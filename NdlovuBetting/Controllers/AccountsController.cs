using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NdlovuBetting.Data;
using NdlovuBetting.Models;
using NdlovuBetting.Models.ViewModels;

namespace NdlovuBetting.Controllers
{
    // Admin-only account management.
    [Authorize(Roles = "Admin")]
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Full details for one account: summary + transactions.
        public async Task<IActionResult> Details(int id)
        {
            // Load the account, its owner, and its transactions.
            var account = await _db.Accounts
                .Include(a => a.User)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
                return NotFound();

            var vm = new AccountDetailsViewModel
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                Status = account.Status.ToString(),
                IsClosed = account.Status == AccountStatus.Closed,
                OwnerId = account.UserId,
                OwnerName = account.User != null
                    ? $"{account.User.FirstName} {account.User.Surname}"
                    : "Unknown",
                Transactions = account.Transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ThenByDescending(t => t.Id)
                    .Select(t => new TransactionRow
                    {
                        Id = t.Id,
                        TransactionDate = t.TransactionDate,
                        Type = t.Type.ToString(),   // "Credit" or "Debit"
                        Description = t.Description,
                        SignedAmount = t.SignedAmount,
                        CapturedAt = t.CapturedAt
                    })
                    .ToList()
            };

            return View(vm);
        }
    }
}