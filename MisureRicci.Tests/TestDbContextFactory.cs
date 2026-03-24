using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;

namespace MisureRicci.Tests;

internal sealed class TestDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    private bool _disposed;

    public TestDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public ApplicationDbContext CreateContext(Services.ITenantService? tenantService = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new ApplicationDbContext(options, tenantService);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _connection.Dispose();
        _disposed = true;
    }
}