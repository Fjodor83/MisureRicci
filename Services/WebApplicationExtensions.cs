using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.Options;
using Microsoft.Extensions.Options;
using Serilog;

namespace MisureRicci.Services
{
    public static class WebApplicationExtensions
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            // TODO: Bisogna rivedere questo metodo in modo che funzioni anche in produzione,
            // dove le migrazioni automatiche non sono sempre una buona idea.
            // Per ora, se siamo in produzione, assumiamo che il DB sia già pronto
            // e skippiamo tutto.
            return;

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var dbContext = services.GetRequiredService<ApplicationDbContext>();

                // Applica automaticamente tutte le migration pendenti (SQL Server)
                await dbContext.Database.MigrateAsync();

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                foreach (var roleName in ApplicationRoles.All)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                }

                // Bootstrap admin da configurazione / variabili d'ambiente
                var adminOptions = services.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;
                if (adminOptions.Enabled && !string.IsNullOrWhiteSpace(adminOptions.Email))
                {
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var adminUser = await userManager.FindByEmailAsync(adminOptions.Email);
                    if (adminUser == null)
                    {
                        adminUser = new ApplicationUser
                        {
                            UserName = adminOptions.Email,
                            Email = adminOptions.Email,
                            EmailConfirmed = true,
                            NomeCompleto = adminOptions.NomeCompleto,
                            Ruolo = ApplicationRoles.Admin,
                            Attivo = true
                        };

                        var result = await userManager.CreateAsync(adminUser, adminOptions.Password!);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
                            Log.Information("Admin user {Email} created via bootstrap.", adminOptions.Email);
                        }
                        else
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            Log.Error("Failed to create admin user {Email}: {Errors}", adminOptions.Email, errors);
                        }
                    }
                    else
                    {
                        // Se l'utente esiste già, assicuriamoci che abbia la password corretta (quella in Railway)
                        // e che sia Admin. Utile se il primo deploy è fallito a metà.
                        var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                        var result = await userManager.ResetPasswordAsync(adminUser, token, adminOptions.Password!);
                        
                        if (result.Succeeded)
                        {
                            if (!await userManager.IsInRoleAsync(adminUser, ApplicationRoles.Admin))
                            {
                                await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
                            }
                            Log.Information("Admin user {Email} password and role updated via bootstrap.", adminOptions.Email);
                        }
                    }
                }

                await MeasurementTypeSeeder.SeedDefaultsAsync(dbContext);

                var imageStorage = services.GetRequiredService<IMeasurementTypeImageStorageService>();
                var migrated = await imageStorage.MigrateLegacyImagesAsync(dbContext);
                if (migrated > 0)
                    Log.Information("Migrated {Count} legacy images to protected storage.", migrated);
            }
            catch (Exception ex)
            {
                // Non rilanciare: il server deve avviarsi anche se l'init del DB fallisce,
                // altrimenti Railway non può mai completare l'healthcheck e il deploy fallisce sempre.
                // L'app riproverà le migrazioni al prossimo restart o può essere fixata senza downtime.
                Log.Error(ex, "An error occurred during database initialization. The app will continue to start.");
            }
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var nonce = GenerateNonce();
                context.Items["CSP-Nonce"] = nonce;

                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
                context.Response.Headers.Append(
                    "Content-Security-Policy",
                    $"default-src 'none'; " +
                    $"connect-src 'self' http://localhost:* ws://localhost:*; " +
                    $"script-src 'self' 'nonce-{nonce}' https://cdn.jsdelivr.net; " +
                    $"style-src 'self' 'nonce-{nonce}' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
                    "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
                    "img-src 'self' data:; " +
                    "frame-ancestors 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'; " +
                    "upgrade-insecure-requests; " +
                    "block-all-mixed-content;");

                await next();
            });
        }

        private static string GenerateNonce()
        {
            var bytes = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}