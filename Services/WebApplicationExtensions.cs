using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.Options;
using MisureRicci.Services;
using Microsoft.Extensions.Options;
using Serilog;

namespace Microsoft.AspNetCore.Builder
{
    public static class WebApplicationExtensions
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var dbContext = services.GetRequiredService<ApplicationDbContext>();
                
                // 1. Ensure schema compatibility (handles renames/column additions for legacy DBs)
                await SqlServerSchemaCompatibilityBootstrapper.EnsureCompatibleAsync(dbContext);

                // 2. Ensure all migrations are applied
                await dbContext.Database.MigrateAsync();

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                foreach (var roleName in ApplicationRoles.All)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // 3. Seed Admin User
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
                }

                await MeasurementTypeSeeder.SeedDefaultsAsync(dbContext);

                var imageStorage = services.GetRequiredService<IMeasurementTypeImageStorageService>();
                var migrated = await imageStorage.MigrateLegacyImagesAsync(dbContext);
                if (migrated > 0)
                {
                    Log.Information("Migrated {Count} legacy images to protected storage.", migrated);
                }
            }
            catch (Exception ex)
            {
                const string message = "An error occurred during database initialization.";
                Log.Error(ex, message);
                throw new InvalidOperationException(message, ex);
            }
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
                context.Response.Headers.Append(
                    "Content-Security-Policy",
                    "default-src 'none'; " +
                    "connect-src 'self' http://localhost:* ws://localhost:*; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
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
    }
}
