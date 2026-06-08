using Microsoft.AspNetCore.Identity;
using Staybnb.Models;

namespace Staybnb.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            {
                "Guest",
                "Host",
                "Admin",
                "SuperAdmin"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }

        public static async Task SeedSuperAdminAsync(
            IServiceProvider serviceProvider)
        {
            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string email = "superadmin@staybnb.com";
            string password = "Admin123!";

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "System Super Admin"
                };

                await userManager.CreateAsync(user, password);

                await userManager.AddToRoleAsync(user, "SuperAdmin");
                await userManager.AddToRoleAsync(user, "Admin");
                await userManager.AddToRoleAsync(user, "Host");
                await userManager.AddToRoleAsync(user, "Guest");
            }
        }
    }
}