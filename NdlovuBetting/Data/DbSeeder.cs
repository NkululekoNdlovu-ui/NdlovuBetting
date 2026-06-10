using Microsoft.AspNetCore.Identity;
using NdlovuBetting.Models;

namespace NdlovuBetting.Data
{
    public static class DbSeeder
    {
        // The default admin login. Change the password after first login in real use.
        private const string AdminEmail = "admin@ndlovu.com";
        private const string AdminPassword = "Admin@123";

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 1) Make sure both roles exist.
            foreach (var role in new[] { Roles.Admin, Roles.Client })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2) Make sure one admin login exists.
            var admin = await userManager.FindByEmailAsync(AdminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, AdminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, Roles.Admin);
            }
        }
    }
}