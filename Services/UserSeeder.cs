using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MisureRicci.Models;
using System.Security.Cryptography;

namespace MisureRicci.Services
{
    public static class UserSeeder
    {
        /// <summary>
        /// Crea l'utente admin di default se non esiste.
        /// La password viene letta da <c>BootstrapAdmin:Password</c> nella configurazione.
        /// In produzione usare la variabile d'ambiente <c>BootstrapAdmin__Password</c>
        /// (double-underscore per la gerarchia di configurazione .NET).
        /// Se la configurazione è assente, genera una password casuale non loggata.
        /// </summary>
        public static async Task SeedAdminUserAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
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

                var password = configuration["BootstrapAdmin:Password"];
                if (string.IsNullOrWhiteSpace(password))
                {
                    password = GenerateRandomPassword();
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

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }

        private static string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var bytes = new byte[20];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var password = new char[20];
            for (var i = 0; i < password.Length; i++)
            {
                password[i] = chars[bytes[i] % chars.Length];
            }
            return new string(password);
        }
    }
}
