using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.Options;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MisureRicci.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProjectDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            return services;
        }

        public static IServiceCollection AddProjectIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
            .AddDefaultUI()
            .AddDefaultTokenProviders();

            services.AddScoped<SignInManager<ApplicationUser>, CustomSignInManager>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = "__Secure-SR-Auth";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            });

            return services;
        }

        public static IProjectServiceBuilder AddProjectServices(this IServiceCollection services)
        {
            return AddProjectServices(services, null);
        }

        public static IProjectServiceBuilder AddProjectServices(this IServiceCollection services, IConfiguration? config)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IAuditService, AuditService>();

            var storageOptions = new StorageOptions();
            config?.GetSection(StorageOptions.SectionName).Bind(storageOptions);
            var storageRoot = ResolveStorageRoot(storageOptions.LocalBasePath);

            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IMeasurementService, MeasurementService>();
            services.AddScoped<IMeasurementRegistryService, MeasurementService>();
            services.AddScoped<ILegacyMeasurementService, MeasurementService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddSingleton<IPdfStorageService>(sp =>
            {
                var basePath = Path.Combine(storageRoot, "PdfDossiers");
                return new LocalPdfStorageProvider(basePath);
            });
            services.AddScoped<ICustomMeasurementService, CustomMeasurementService>();
            services.AddScoped<IMeasurementTypeImageStorageService, MeasurementTypeImageStorageService>();

            // File storage provider (configurabile via Storage:Provider)
            var storageProvider = storageOptions.Provider;
            if (string.Equals(storageProvider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<IFileStorageProvider, AzureBlobStorageProvider>();
            }
            else
            {
                services.AddSingleton<IFileStorageProvider>(sp =>
                {
                    var basePath = Path.Combine(storageRoot, "Files");
                    return new LocalFileStorageProvider(basePath);
                });
            }

            services.AddScoped<IFabricService, FabricService>();
            services.AddScoped<ICommessaQueryService, CommessaQueryService>();
            services.AddScoped<ICommessaCommandService, CommessaCommandService>();
            services.AddScoped<ICommessaMisuraLinkService, CommessaMisuraLinkService>();
            services.AddScoped<ICommessaService, CommessaService>();
            services.AddScoped<INegozioService, NegozioService>();
            services.AddScoped<ILegacyMeasurementUiService, LegacyMeasurementUiService>();
            services.AddScoped<ILegacyMeasurementConverter, LegacyMeasurementConverter>();

            // AGGIUNTO: Email service per ASP.NET Identity
            services.AddScoped<IEmailSender, EmailService>();
            if (config != null)
            {
                services.Configure<SmtpSettings>(config.GetSection(SmtpSettings.SectionName));
            }

            return new ProjectServiceBuilder(services);
        }

        private static string ResolveStorageRoot(string? configuredPath)
        {
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return Path.GetFullPath(configuredPath);
            }

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "SecureUploads"));
        }

        public static IProjectServiceBuilder AddProjectRateLimiters(this IProjectServiceBuilder builder)
        {
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("login", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 20,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                // AGGIUNTO: Policy per API generiche
                options.AddPolicy("api", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 60,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                // AGGIUNTO: Policy per export CSV (limite più restrittivo)
                options.AddPolicy("export", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));
            });
            return builder;
        }
    }

    public interface IProjectServiceBuilder
    {
        IServiceCollection Services { get; }
    }

    internal class ProjectServiceBuilder : IProjectServiceBuilder
    {
        public IServiceCollection Services { get; }
        public ProjectServiceBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}