using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MisureRicci.Models;
using MisureRicci.Models.Options;
using MisureRicci.Services;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7);

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
        .AddProjectServices(builder.Configuration)
        .AddProjectRateLimiters();

    builder.Services.AddOptions<BootstrapAdminOptions>()
        .BindConfiguration(BootstrapAdminOptions.SectionName)
        .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<BootstrapAdminOptions>, BootstrapAdminOptionsValidator>();

    // Health checks
    builder.Services.AddHealthChecks()
        .AddSqlServer(connectionString, name: "sqlserver", tags: ["ready"]);

    builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build());

    var app = builder.Build();

    var startupDbInitEnabled = builder.Configuration.GetValue<bool?>("StartupDatabaseInit:Enabled") ?? true;

    if (startupDbInitEnabled)
    {
        try
        {
            await app.InitializeDatabaseAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "InitializeDatabaseAsync fallito. Continuazione avvio applicazione.");
        }
    }
    else
    {
        Log.Information("Startup database initialization disabilitata da configurazione (StartupDatabaseInit:Enabled=false).");
    }

    // In Development abilitiamo HTTPS redirect; in production usiamo exception handler.
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

    // Healthcheck applicativo
    app.MapGet("/health", () => Results.Ok("Healthy"));

    // Readiness check SQL Server
    app.MapHealthChecks("/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    await app.RunAsync();
}
catch (HostAbortedException ex)
{
    var isEfDesignTime = AppDomain.CurrentDomain
        .GetAssemblies()
        .Any(assembly => string.Equals(assembly.GetName().Name, "Microsoft.EntityFrameworkCore.Design", StringComparison.Ordinal));

    if (!isEfDesignTime)
    {
        Log.Warning(ex, "Host aborted during startup. Application is shutting down.");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

