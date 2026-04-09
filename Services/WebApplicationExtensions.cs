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

                // Su database esistente consentiamo comunque bootstrap ruoli/admin,
                // utile se BootstrapAdmin viene abilitato dopo il primo avvio.
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

            // Fase 2/3: Ruoli + bootstrap admin
            await EnsureRolesAndBootstrapAdminAsync(services);

            // Fase 4: Seed tipi misura predefiniti (solo primo avvio)
            try
            {
                await MeasurementTypeSeeder.SeedDefaultsAsync(dbContext);
                Log.Information("Seed tipi misura completato.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Seed tipi misura fallito. Continuazione avvio.");
            }

            // Fase 5: Migrazione immagini legacy (solo primo avvio, non in test)
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
            // Seed ruoli applicazione
            try
            {
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                foreach (var roleName in ApplicationRoles.All)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        Log.Information("Ruolo '{RoleName}' creato.", roleName);
                    }
                }
                Log.Information("Seed ruoli completato.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Seed ruoli fallito. Continuazione avvio.");
            }

            // Bootstrap admin da configurazione (se abilitato)
            try
            {
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
                            Log.Information("Admin user {Email} creato tramite bootstrap.", adminOptions.Email);
                        }
                        else
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            Log.Warning("Creazione admin {Email} fallita: {Errors}", adminOptions.Email, errors);
                        }
                    }
                    else
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                        var result = await userManager.ResetPasswordAsync(adminUser, token, adminOptions.Password!);

                        if (result.Succeeded)
                        {
                            if (!await userManager.IsInRoleAsync(adminUser, ApplicationRoles.Admin))
                            {
                                await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
                            }
                            Log.Information("Admin user {Email} password e ruolo aggiornati tramite bootstrap.", adminOptions.Email);
                        }
                    }
                }
                else
                {
                    Log.Information("Bootstrap admin disabilitato o email non configurata.");
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Bootstrap admin fallito. Continuazione avvio.");
            }
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