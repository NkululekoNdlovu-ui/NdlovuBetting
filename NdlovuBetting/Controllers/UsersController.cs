using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NdlovuBetting.Data;
using NdlovuBetting.Models;
using NdlovuBetting.Models.ViewModels;

using NdlovuBetting.Models.Dtos;

namespace NdlovuBetting.Controllers
{
    // Everything in here is admin-only.
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // The users list page (with search + pagination).
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int pageSize = 10;

            var profiles = await _db.AppUsers
                .Include(u => u.Accounts)
                .ToListAsync();

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminEmails = adminUsers
                .Select(a => a.Email)
                .Where(e => e != null)
                .Select(e => e!.ToLower())
                .ToHashSet();

            var allRows = profiles.Select(p => new UserRow
            {
                Id = p.Id,
                FullName = $"{p.FirstName} {p.Surname}",
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                IdNumber = p.IdNumber,
                Role = (p.Email != null && adminEmails.Contains(p.Email.ToLower())) ? "Admin" : "Client",
                AccountCount = p.Accounts.Count,
                TotalBalance = p.Accounts.Sum(a => a.Balance),
                CreatedAt = p.CreatedAt
            }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();

                var profileIdsByAccount = await _db.Accounts
                    .Where(a => a.AccountNumber.ToLower().Contains(term))
                    .Select(a => a.UserId)
                    .ToListAsync();

                allRows = allRows.Where(r =>
                    r.IdNumber.ToLower().Contains(term) ||
                    r.FullName.ToLower().Contains(term) ||
                    (r.Email != null && r.Email.ToLower().Contains(term)) ||
                    profileIdsByAccount.Contains(r.Id)
                ).ToList();
            }

            var statsSource = allRows;

            var totalCount = allRows.Count;
            var pagedRows = allRows
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new UsersListViewModel
            {
                Users = pagedRows,
                Search = search,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,

                TotalUsers = statsSource.Count,
                TotalClients = statsSource.Count(r => r.Role == "Client"),
                TotalAccounts = statsSource.Sum(r => r.AccountCount),
                GrandTotalBalance = statsSource.Sum(r => r.TotalBalance)
            };

            return View(vm);
        }

        // Full details for one user: their profile + their accounts.
        public async Task<IActionResult> Details(int id)
        {
            var profile = await _db.AppUsers
                .Include(u => u.Accounts)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (profile == null)
                return NotFound();

            string role = "Client";
            if (profile.Email != null)
            {
                var appUser = await _userManager.FindByEmailAsync(profile.Email);
                if (appUser != null && await _userManager.IsInRoleAsync(appUser, "Admin"))
                    role = "Admin";
            }

            var vm = new UserDetailsViewModel
            {
                Id = profile.Id,
                FirstName = profile.FirstName,
                Surname = profile.Surname,
                IdNumber = profile.IdNumber,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                Role = role,
                CreatedAt = profile.CreatedAt,
                Accounts = profile.Accounts
                    .OrderByDescending(a => a.Id)
                    .Select(a => new AccountRow
                    {
                        Id = a.Id,
                        AccountNumber = a.AccountNumber,
                        Balance = a.Balance,
                        Status = a.Status.ToString()
                    })
                    .ToList()
            };

            return View(vm);
        }

        // Show the Create Admin form.
        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View(new CreateAdminDto());
        }

        // Handle the Create Admin form submission.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(CreateAdminDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // Email must not already be in use.
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
            {
                ModelState.AddModelError(string.Empty, "That email is already registered.");
                return View(dto);
            }

            // Split the full name into first + surname.
            var parts = dto.FullName.Trim().Split(' ', 2);
            var firstName = parts[0];
            var surname = parts.Length > 1 ? parts[1] : "";

            // 1) Create the Identity login.
            var appUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(appUser, dto.Password);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(dto);
            }

            // 2) Put them in the Admin role.
            await _userManager.AddToRoleAsync(appUser, "Admin");

            // 3) Create a profile row so they appear in the Users List.
            //    Admins have no real ID number, so use a unique placeholder.
            var placeholderId = "ADMIN-" + DateTime.UtcNow.Ticks.ToString().Substring(10);

            var profile = new User
            {
                FirstName = firstName,
                Surname = surname,
                Email = dto.Email,
                IdNumber = placeholderId,
                CreatedAt = DateTime.Now
            };

            _db.AppUsers.Add(profile);
            await _db.SaveChangesAsync();

            // Link the login to the profile.
            appUser.UserId = profile.Id;
            await _userManager.UpdateAsync(appUser);

            TempData["Success"] = $"Admin '{dto.Email}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}