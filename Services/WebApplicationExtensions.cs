using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            var startupDbInitEnabled = app.Configuration.GetValue<bool?>("StartupDatabaseInit:Enabled") ?? true;
            if (!startupDbInitEnabled)
            {
                Log.Information("InitializeDatabaseAsync saltato: StartupDatabaseInit:Enabled=false.");
                return;
            }

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var env = app.Environment;
            var dbContext = services.GetRequiredService<ApplicationDbContext>();

            // Fase 1: inizializzazione solo al primo avvio (DB assente).
            var databaseExists = false;
            try
            {
                databaseExists = await dbContext.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Verifica esistenza database fallita. Continuazione avvio senza inizializzazione.");
                return;
            }

            if (databaseExists)
            {
                Log.Information("Database già esistente: creazione schema saltata.");
                await EnsureRolesAndBootstrapAdminAsync(services);
                return;
            }

            try
            {
                var created = await dbContext.Database.EnsureCreatedAsync();
                if (created)
                {
                    Log.Information("Database creato con successo (primo avvio).");
                }
                else
                {
                    Log.Warning("EnsureCreatedAsync non ha creato il database. Continuazione avvio.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Creazione database fallita al primo avvio. Continuazione avvio.");
                return;
            }

            await EnsureRolesAndBootstrapAdminAsync(services);

            try
            {
                await MeasurementTypeSeeder.SeedDefaultsAsync(dbContext);
                Log.Information("Seed tipi misura completato.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Seed tipi misura fallito. Continuazione avvio.");
            }

            try
            {
                var isTestEnv = env.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase);
                if (!isTestEnv)
                {
                    var imageStorage = services.GetRequiredService<IMeasurementTypeImageStorageService>();
                    var migrated = await imageStorage.MigrateLegacyImagesAsync(dbContext);
                    if (migrated > 0)
                    {
                        Log.Information("Migrate {Count} immagini legacy in storage protetto.", migrated);
                    }
                    else
                    {
                        Log.Information("Nessuna immagine legacy da migrare.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Migrazione immagini legacy fallita. Continuazione avvio.");
            }

            Log.Information("Inizializzazione database completata.");
        }

        private static async Task EnsureRolesAndBootstrapAdminAsync(IServiceProvider services)
        {
            await SeedApplicationRolesAsync(services);
            await BootstrapAdminUserAsync(services);
        }

        private static async Task SeedApplicationRolesAsync(IServiceProvider services)
        {
            try
            {
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                foreach (var roleName in ApplicationRoles.All)
                {
                    await EnsureRoleExistsAsync(roleManager, roleName);
                }
                Log.Information("Seed ruoli completato.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Seed ruoli fallito. Continuazione avvio.");
            }
        }

        private static async Task EnsureRoleExistsAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                return;

            await roleManager.CreateAsync(new IdentityRole(roleName));
            Log.Information("Ruolo '{RoleName}' creato.", roleName);
        }

        private static async Task BootstrapAdminUserAsync(IServiceProvider services)
        {
            try
            {
                var adminOptions = services.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;

                // Pattern matching per ottenere email non-nullabile
                if (!(adminOptions.Enabled && adminOptions.Email is string email && !string.IsNullOrWhiteSpace(email)))
                {
                    Log.Information("Bootstrap admin disabilitato o email non configurata.");
                    return;
                }

                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var adminUser = await userManager.FindByEmailAsync(email);

                if (adminUser == null)
                {
                    await CreateNewAdminUserAsync(userManager, adminOptions, email);
                }
                else
                {
                    await UpdateExistingAdminUserAsync(userManager, adminUser, adminOptions, email);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Bootstrap admin fallito. Continuazione avvio.");
            }
        }

        private static async Task CreateNewAdminUserAsync(
            UserManager<ApplicationUser> userManager,
            BootstrapAdminOptions options,
            string email)
        {
            var adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NomeCompleto = options.NomeCompleto,
                Ruolo = ApplicationRoles.Admin,
                Attivo = true
            };

            var result = await userManager.CreateAsync(adminUser, options.Password!);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Log.Warning("Creazione admin {Email} fallita: {Errors}", email, errors);
                return;
            }

            await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
            Log.Information("Admin user {Email} creato tramite bootstrap.", email);
        }

        private static async Task UpdateExistingAdminUserAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser adminUser,
            BootstrapAdminOptions options,
            string email)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var result = await userManager.ResetPasswordAsync(adminUser, token, options.Password!);

            if (!result.Succeeded)
                return;

            if (!await userManager.IsInRoleAsync(adminUser, ApplicationRoles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
            }

            Log.Information("Admin user {Email} password e ruolo aggiornati tramite bootstrap.", email);
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            var isDevelopment = env.IsDevelopment();

            return app.Use(async (context, next) =>
            {
                var nonce = GenerateNonce();
                context.Items["CSP-Nonce"] = nonce;

                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

                var connectSrc = isDevelopment
                    ? "'self' http://localhost:* ws://localhost:*"
                    : "'self'";

                // ⚠️ Per evitare S5725, si consiglia di servire Bootstrap Icons e Google Fonts localmente.
                // Vedi note nella risposta per come adattare la CSP se si mantengono le CDN.
                context.Response.Headers.Append(
                    "Content-Security-Policy",
                    $"default-src 'none'; " +
                    $"connect-src {connectSrc}; " +
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