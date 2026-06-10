using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NdlovuBetting.Data;
using NdlovuBetting.Models;
using NdlovuBetting.Models.Dtos;
using NdlovuBetting.Models.ViewModels;

namespace NdlovuBetting.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        // Show the signup form.
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterDto());
        }

        // Handle the signup form submission.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            bool idTaken = await _db.AppUsers.AnyAsync(u => u.IdNumber == dto.IdNumber);
            if (idTaken)
            {
                ModelState.AddModelError(string.Empty, $"A user with ID number {dto.IdNumber} already exists.");
                return View(dto);
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            var profile = new User
            {
                IdNumber = dto.IdNumber,
                FirstName = dto.FirstName,
                Surname = dto.Surname,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                CreatedAt = DateTime.Now
            };
            _db.AppUsers.Add(profile);
            await _db.SaveChangesAsync();

            var wallet = new Account
            {
                AccountNumber = await GenerateWalletNumberAsync(),
                UserId = profile.Id,
                Status = AccountStatus.Open,
                CreatedAt = DateTime.Now
            };
            wallet.SetBalance(0m);
            _db.Accounts.Add(wallet);
            await _db.SaveChangesAsync();

            var appUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                UserId = profile.Id
            };
            var result = await _userManager.CreateAsync(appUser, dto.Password);
            if (!result.Succeeded)
            {
                await tx.RollbackAsync();
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(dto);
            }

            await _userManager.AddToRoleAsync(appUser, Roles.Client);
            await tx.CommitAsync();

            await _signInManager.SignInAsync(appUser, isPersistent: false);
            TempData["Success"] = "Welcome! Your account has been created.";
            return RedirectToAction("Dashboard", "Account");
        }

        // Show the login form.
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginDto());
        }

        // Handle the login form submission.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _signInManager.PasswordSignInAsync(
                dto.Email, dto.Password, dto.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(dto);
            }

            // Find the user to check their role.
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // Admins go to the admin area; clients go to their dashboard.
            if (user != null && await _userManager.IsInRoleAsync(user, Roles.Admin))
                return RedirectToAction("Index", "Users");

            return RedirectToAction("Dashboard", "Account");
        }
        // The page a logged-in client lands on.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var vm = await BuildWalletPageAsync();
            if (vm == null) return NotFound();
            return View(vm);
        }

        // Handle a deposit or withdrawal from the wallet popup.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Move(WalletPageViewModel vm)
        {
            var movement = vm.Movement;

            if (movement.Amount <= 0)
            {
                TempData["Error"] = "Amount must be greater than zero.";
                return RedirectToAction(nameof(Dashboard));
            }

            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = await _userManager.FindByIdAsync(appUserId!);
            if (appUser?.UserId == null) return NotFound();

            var wallet = await _db.Accounts.FirstOrDefaultAsync(a => a.UserId == appUser.UserId.Value);
            if (wallet == null)
            {
                TempData["Error"] = "No wallet found for your account.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (wallet.IsClosed)
            {
                TempData["Error"] = "This wallet is closed.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (movement.Action == WalletAction.Withdraw && movement.Amount > wallet.Balance)
            {
                TempData["Error"] = "Insufficient funds for this withdrawal.";
                return RedirectToAction(nameof(Dashboard));
            }

            var type = movement.Action == WalletAction.Deposit
                ? TransactionType.Credit
                : TransactionType.Debit;
            var label = movement.Action == WalletAction.Deposit ? "Deposit" : "Withdrawal";

            await using var tx = await _db.Database.BeginTransactionAsync();

            var transaction = new Transaction
            {
                AccountId = wallet.Id,
                Amount = movement.Amount,
                Type = type,
                Description = label,
                TransactionDate = DateTime.Now,
                CapturedAt = DateTime.Now
            };
            _db.Transactions.Add(transaction);

            wallet.SetBalance(wallet.Balance + transaction.SignedAmount);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = $"{label} of {movement.Amount:C} completed.";
            return RedirectToAction(nameof(Dashboard));
        }

        // Log the user out and send them to the homepage.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Save profile changes from the profile popup.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(WalletPageViewModel vm)
        {
            var dto = vm.Profile;

            // Basic validation: first name and surname are required.
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.Surname))
            {
                TempData["Error"] = "First name and surname are required.";
                return RedirectToAction(nameof(Dashboard));
            }

            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = await _userManager.FindByIdAsync(appUserId!);
            if (appUser?.UserId == null) return NotFound();

            var profile = await _db.AppUsers.FirstOrDefaultAsync(u => u.Id == appUser.UserId.Value);
            if (profile == null) return NotFound();

            // Update the editable fields (ID number is NOT editable).
            profile.FirstName = dto.FirstName;
            profile.Surname = dto.Surname;
            profile.Email = dto.Email;
            profile.PhoneNumber = dto.PhoneNumber;
            profile.Address = dto.Address;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Your profile has been updated.";
            return RedirectToAction(nameof(Dashboard));
        }

        // Makes a unique wallet number like WAL-000123.
        private async Task<string> GenerateWalletNumberAsync()
        {
            string number;
            do
            {
                number = "WAL-" + Random.Shared.Next(0, 1_000_000).ToString("D6");
            }
            while (await _db.Accounts.AnyAsync(a => a.AccountNumber == number));
            return number;
        }

        // Loads the signed-in client's profile, wallet and history into the ViewModel.
        private async Task<WalletPageViewModel?> BuildWalletPageAsync()
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (appUserId == null) return null;

            var appUser = await _userManager.FindByIdAsync(appUserId);
            if (appUser?.UserId == null) return null;

            var profile = await _db.AppUsers.FirstOrDefaultAsync(u => u.Id == appUser.UserId.Value);
            if (profile == null) return null;

            var wallet = await _db.Accounts.FirstOrDefaultAsync(a => a.UserId == profile.Id);

            var history = new List<Transaction>();
            if (wallet != null)
            {
                history = await _db.Transactions
                    .Where(t => t.AccountId == wallet.Id)
                    .OrderByDescending(t => t.TransactionDate)
                    .ThenByDescending(t => t.Id)
                    .ToListAsync();
            }

            return new WalletPageViewModel
            {
                FirstName = profile.FirstName,
                Surname = profile.Surname,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                IdNumber = profile.IdNumber,
                CreatedAt = profile.CreatedAt,

                AccountNumber = wallet?.AccountNumber ?? "—",
                Balance = wallet?.Balance ?? 0m,
                IsClosed = wallet?.IsClosed ?? false,

                Transactions = history,

                Profile = new ProfileDto
                {
                    FirstName = profile.FirstName,
                    Surname = profile.Surname,
                    Email = profile.Email,
                    PhoneNumber = profile.PhoneNumber,
                    Address = profile.Address
                }
            };
        }
    }
}