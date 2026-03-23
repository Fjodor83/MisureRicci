using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

    builder.Services.AddScoped<MisureRicci.Services.IClienteService, MisureRicci.Services.ClienteService>();
    builder.Services.AddScoped<MisureRicci.Services.IMeasurementService, MisureRicci.Services.MeasurementService>();
    builder.Services.AddScoped<MisureRicci.Services.IPdfService, MisureRicci.Services.PdfService>();
    builder.Services.AddScoped<MisureRicci.Services.ICustomMeasurementService, MisureRicci.Services.CustomMeasurementService>();
    builder.Services.AddScoped<MisureRicci.Services.ICommessaService, MisureRicci.Services.CommessaService>();
    builder.Services.AddScoped<MisureRicci.Services.INegozioService, MisureRicci.Services.NegozioService>();
    // IUtenteService/UtenteService are obsolete — UtentiController now uses UserManager<ApplicationUser> directly.

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.LogoutPath = "/Identity/Account/Logout";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    });

    builder.Services.AddAuthorization(options => {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var bootstrapEnabled = builder.Configuration.GetValue<bool>("BootstrapAdmin:Enabled");
        var bootstrapEmail = builder.Configuration["BootstrapAdmin:Email"];
        var bootstrapPassword = builder.Configuration["BootstrapAdmin:Password"];
        var bootstrapFullName = builder.Configuration["BootstrapAdmin:NomeCompleto"] ?? "Amministratore Sistema";

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (bootstrapEnabled &&
            !string.IsNullOrWhiteSpace(bootstrapEmail) &&
            !string.IsNullOrWhiteSpace(bootstrapPassword) &&
            await userManager.FindByEmailAsync(bootstrapEmail) == null)
        {
            var admin = new ApplicationUser {
                UserName = bootstrapEmail,
                Email = bootstrapEmail,
                NomeCompleto = bootstrapFullName,
                Ruolo = "Admin",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, bootstrapPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
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

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();

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
