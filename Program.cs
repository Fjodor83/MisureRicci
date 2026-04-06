using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MisureRicci.Models.Options;
using MisureRicci.Services;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;

// ── Serilog: solo Console in produzione (Railway ha filesystem efimero) ───────
var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
                || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console();

// Aggiungi file logging solo fuori da Railway (sviluppo locale)
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

    // ── Connection String: Railway inietta DATABASE_URL come variabile d'ambiente
    // Supporta sia il formato Railway (postgresql://...) sia il formato Npgsql standard
    var connectionString = GetConnectionString(builder.Configuration);
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string non trovata. Imposta DATABASE_URL su Railway o " +
            "ConnectionStrings:DefaultConnection nei secrets locali.");
    }

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMemoryCache();

    builder.Services
        .AddProjectDatabase(connectionString)
        .AddProjectIdentity()
        .AddProjectServices()
        .AddProjectRateLimiters();

    // ── Railway: la porta viene assegnata tramite $PORT ───────────────────────
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

    builder.Services.AddOptions<BootstrapAdminOptions>()
        .BindConfiguration(BootstrapAdminOptions.SectionName)
        .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<BootstrapAdminOptions>, BootstrapAdminOptionsValidator>();

    // ── Health check PostgreSQL ───────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddCheck<PostgresHealthCheck>("postgres", tags: new[] { "ready" });

    builder.Services.AddAuthorization(options => {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    var app = builder.Build();

    await app.InitializeDatabaseAsync();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseSecurityHeaders();

    // Railway gestisce TLS via reverse proxy — HTTPS redirect solo in locale
    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();

    app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
    app.MapHealthChecks("/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    app.Run();
}
catch (HostAbortedException)
{
    // Atteso durante operazioni design-time (migrations ecc.)
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// ── Helper: normalizza DATABASE_URL di Railway in connection string Npgsql ────
static string? GetConnectionString(IConfiguration configuration)
{
    // 1. Prova prima la variabile DATABASE_URL che Railway imposta automaticamente
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        return ConvertDatabaseUrlToNpgsql(databaseUrl);
    }

    // 2. Fallback: variabile custom POSTGRES_CONNECTION o ConnectionStrings standard
    var custom = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
    if (!string.IsNullOrWhiteSpace(custom))
        return custom;

    return configuration.GetConnectionString("DefaultConnection");
}

// Converte postgresql://user:pass@host:port/dbname → formato Npgsql
static string ConvertDatabaseUrlToNpgsql(string databaseUrl)
{
    // Railway usa il formato: postgresql://user:password@host:port/database
    if (!databaseUrl.StartsWith("postgresql://") && !databaseUrl.StartsWith("postgres://"))
        return databaseUrl; // già in formato Npgsql, restituisci as-is

    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var user = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}