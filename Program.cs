using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MisureRicci.Models;
using MisureRicci.Models.Options;
using MisureRicci.Services;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;

var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
                || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console();

if (!isProduction)
{
    loggerConfig.WriteTo.File(
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7);
}

Log.Logger = loggerConfig.CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    QuestPDF.Settings.License = LicenseType.Community;

    // Connessione SQL Server
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                          ?? Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION");
    
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Connection string 'DefaultConnection' non trovata.");

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMemoryCache();

    builder.Services
        .AddProjectDatabase(connectionString)
        .AddProjectIdentity()
        .AddProjectServices()
        .AddProjectRateLimiters();

    // Configurazione porta (Railway usa PORT, locale usa default)
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

    builder.Services.AddOptions<BootstrapAdminOptions>()
        .BindConfiguration(BootstrapAdminOptions.SectionName)
        .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<BootstrapAdminOptions>, BootstrapAdminOptionsValidator>();

    // Health checks
    builder.Services.AddHealthChecks()
        .AddSqlServer(connectionString, name: "sqlserver", tags: ["ready"]);

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await UserSeeder.SeedAdminUserAsync(userManager, roleManager);
    }

    await app.InitializeDatabaseAsync();

    // ❌ Niente HSTS su Railway
    // ❌ Niente HTTPS redirect su Railway
    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
    }

    app.UseSecurityHeaders();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();

    // Healthcheck per Railway → sempre 200 OK
    app.MapGet("/health", () => Results.Ok("Healthy"));

    // Readiness check SQL Server
    app.MapHealthChecks("/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    app.Run();
}
catch (HostAbortedException)
{
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

