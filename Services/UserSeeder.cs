using Microsoft.AspNetCore.Identity;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public static class UserSeeder
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            const string adminEmail = "admin@mizurericci.it";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NomeCompleto = "Amministratore di Sistema",
                    Ruolo = "Admin",
                    Attivo = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
