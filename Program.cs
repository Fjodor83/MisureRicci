using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
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

    // Connection string Railway
    var connectionString = GetConnectionString(builder.Configuration);
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Connection string non trovata.");

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMemoryCache();

    builder.Services
        .AddProjectDatabase(connectionString)
        .AddProjectIdentity()
        .AddProjectServices()
        .AddProjectRateLimiters();

    // Railway usa PORT
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

    builder.Services.AddOptions<BootstrapAdminOptions>()
        .BindConfiguration(BootstrapAdminOptions.SectionName)
        .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<BootstrapAdminOptions>, BootstrapAdminOptionsValidator>();

    // Health checks
    builder.Services.AddHealthChecks()
        .AddCheck<PostgresHealthCheck>("postgres", tags: new[] { "ready" });

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    var app = builder.Build();

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

    // Readiness check PostgreSQL
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

static string? GetConnectionString(IConfiguration configuration)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
        return ConvertDatabaseUrlToNpgsql(databaseUrl);

    var custom = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
    if (!string.IsNullOrWhiteSpace(custom))
        return custom;

    return configuration.GetConnectionString("DefaultConnection");
}

static string ConvertDatabaseUrlToNpgsql(string databaseUrl)
{
    if (!databaseUrl.StartsWith("postgresql://") && !databaseUrl.StartsWith("postgres://"))
        return databaseUrl;

    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var user = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}
