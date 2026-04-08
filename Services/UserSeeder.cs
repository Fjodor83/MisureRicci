using Microsoft.AspNetCore.Identity;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public static class UserSeeder
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            const string adminEmail = "admin@misurericci.it";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                foreach (var roleName in ApplicationRoles.All)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                }

                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NomeCompleto = "Amministratore di Sistema",
                    Ruolo = "Admin",
                    Attivo = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "TECHservice123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
