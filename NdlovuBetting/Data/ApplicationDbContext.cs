using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NdlovuBetting.Models;

namespace NdlovuBetting.Data
{
    // This class represents your database. Each DbSet becomes a table.
    // It inherits IdentityDbContext so the login/role tables are included too.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> AppUsers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // sets up the Identity tables first

            // ---- User rules ----
            modelBuilder.Entity<User>(e =>
            {
                // No two users can share the same ID number.
                e.HasIndex(u => u.IdNumber).IsUnique();
                // Speeds up searching by surname.
                e.HasIndex(u => u.Surname);

                e.HasMany(u => u.Accounts)
                 .WithOne(a => a.User!)
                 .HasForeignKey(a => a.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Account rules ----
            modelBuilder.Entity<Account>(e =>
            {
                // No two accounts can share the same account number.
                e.HasIndex(a => a.AccountNumber).IsUnique();

                e.HasMany(a => a.Transactions)
                 .WithOne(t => t.Account!)
                 .HasForeignKey(t => t.AccountId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Transaction rules ----
            modelBuilder.Entity<Transaction>(e =>
            {
                // Speeds up listing an account's transactions by date.
                e.HasIndex(t => new { t.AccountId, t.TransactionDate });
            });

            // ---- Link the login to the user profile (one-to-one) ----
            modelBuilder.Entity<ApplicationUser>(e =>
            {
                e.HasOne(au => au.User)
                 .WithMany()
                 .HasForeignKey(au => au.UserId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}