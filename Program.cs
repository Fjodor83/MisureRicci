using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.Options;
using MisureRicci.Services;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.File(
        new JsonFormatter(),
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    QuestPDF.Settings.License = LicenseType.Community;

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("DefaultConnection non configurata. Usare User Secrets o variabili d'ambiente.");
    }

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
    builder.Services.AddMemoryCache();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services
        .AddOptions<BootstrapAdminOptions>()
        .Bind(builder.Configuration.GetSection(BootstrapAdminOptions.SectionName))
        .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<BootstrapAdminOptions>, BootstrapAdminOptionsValidator>();

    builder.Services.AddHealthChecks()
        .AddCheck<SqlServerHealthCheck>("sqlserver", tags: new[] { "ready" });

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.SignIn.RequireConfirmedAccount = false;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

    builder.Services.AddScoped<SignInManager<ApplicationUser>, CustomSignInManager>();

    builder.Services.AddScoped<MisureRicci.Services.IClienteService, MisureRicci.Services.ClienteService>();
    builder.Services.AddScoped<MisureRicci.Services.IMeasurementService, MisureRicci.Services.MeasurementService>();
    builder.Services.AddScoped<MisureRicci.Services.IMeasurementRegistryService, MisureRicci.Services.MeasurementService>();
    builder.Services.AddScoped<MisureRicci.Services.ILegacyMeasurementService, MisureRicci.Services.MeasurementService>();
    builder.Services.AddScoped<MisureRicci.Services.IDashboardService, MisureRicci.Services.DashboardService>();
    builder.Services.AddScoped<MisureRicci.Services.IPdfService, MisureRicci.Services.PdfService>();
    builder.Services.AddScoped<MisureRicci.Services.ICustomMeasurementService, MisureRicci.Services.CustomMeasurementService>();
    builder.Services.AddScoped<MisureRicci.Services.ICommessaService, MisureRicci.Services.CommessaService>();
    builder.Services.AddScoped<MisureRicci.Services.INegozioService, MisureRicci.Services.NegozioService>();
    builder.Services.AddScoped<MisureRicci.Services.ILegacyMeasurementUiService, MisureRicci.Services.LegacyMeasurementUiService>();
    // IUtenteService/UtenteService are obsolete — UtentiController now uses UserManager<ApplicationUser> directly.
    builder.Services.AddScoped<MisureRicci.Services.ILegacyMeasurementConverter, MisureRicci.Services.LegacyMeasurementConverter>();

    builder.Services.ConfigureApplicationCookie(options =>
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

    builder.Services.AddAuthorization(options => {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("login", httpContext =>
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

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var bootstrapOptions = scope.ServiceProvider.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;

        var bootstrapEnabled = bootstrapOptions.Enabled;
        var bootstrapEmail = bootstrapOptions.Email;
        var bootstrapPassword = bootstrapOptions.Password;
        var bootstrapFullName = bootstrapOptions.NomeCompleto;

        await SqlServerSchemaCompatibilityBootstrapper.EnsureCompatibleAsync(dbContext);

        foreach (var roleName in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        if (bootstrapEnabled &&
            !string.IsNullOrWhiteSpace(bootstrapEmail) &&
            !string.IsNullOrWhiteSpace(bootstrapPassword) &&
            await userManager.FindByEmailAsync(bootstrapEmail) == null)
        {
            var admin = new ApplicationUser {
                UserName = bootstrapEmail,
                Email = bootstrapEmail,
                NomeCompleto = bootstrapFullName,
                Ruolo = ApplicationRoles.Admin,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, bootstrapPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, ApplicationRoles.Admin);
            else
                Log.Warning("Bootstrap admin user creation failed for {Email}: {Errors}", bootstrapEmail, string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        else if (bootstrapEnabled)
        {
            Log.Warning("BootstrapAdmin is enabled but configuration is incomplete or user already exists.");
        }

        await MisureRicci.Services.MeasurementTypeSeeder.SeedDefaultsAsync(dbContext);
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'self'; " +
            "connect-src 'self' ws: wss: http://localhost:* https://localhost:*; " +
            "script-src 'self' cdnjs.cloudflare.com cdn3.devexpress.com; " +
            "style-src 'self' 'unsafe-inline' fonts.googleapis.com cdn.jsdelivr.net cdn3.devexpress.com cdnjs.cloudflare.com; " +
            "font-src 'self' fonts.gstatic.com cdn.jsdelivr.net cdn3.devexpress.com; " +
            "img-src 'self' data:;");

        await next();
    });

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    app.MapHealthChecks("/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    app.Run();
}
catch (HostAbortedException)
{
    // Expected during design-time operations (e.g. EF migrations).
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
