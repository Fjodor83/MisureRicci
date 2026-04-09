using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Services;
using Moq;
using Xunit;

namespace MisureRicci.Tests;

public class DashboardServiceTests
{
    private static IServiceScopeFactory CreateScopeFactory(TestDbContextFactory factory)
    {
        var mock = new Mock<IServiceScopeFactory>();
        mock.Setup(f => f.CreateScope()).Returns(() =>
        {
            var ctx = factory.CreateContext();
            var provider = new Mock<IServiceProvider>();
            provider.Setup(p => p.GetService(typeof(ApplicationDbContext))).Returns(ctx);
            var scope = new Mock<IServiceScope>();
            scope.Setup(s => s.ServiceProvider).Returns(provider.Object);
            scope.Setup(s => s.Dispose()).Callback(() => ctx.Dispose());
            return scope.Object;
        });
        return mock.Object;
    }
    [Fact]
    public async Task GetKpiAsync_NonAdmin_ReturnsOnlyOwnNegozioSnapshot()
    {
        using var factory = new TestDbContextFactory();
        var mockTenant = new Mock<ITenantService>();
        mockTenant.Setup(s => s.IsAdmin()).Returns(false);

        int negozio1Id;

        using (var seedContext = factory.CreateContext())
        {
            var negozio1 = new Negozio
            {
                Nome = "Negozio 1",
                Citta = "Milano",
                Paese = "Italy"
            };
            var negozio2 = new Negozio
            {
                Nome = "Negozio 2",
                Citta = "Roma",
                Paese = "Italy"
            };

            seedContext.Negozi.AddRange(negozio1, negozio2);
            await seedContext.SaveChangesAsync();

            seedContext.Users.AddRange(
                new ApplicationUser
                {
                    Id = "user-n1",
                    UserName = "user1@example.com",
                    NormalizedUserName = "USER1@EXAMPLE.COM",
                    Email = "user1@example.com",
                    NormalizedEmail = "USER1@EXAMPLE.COM",
                    EmailConfirmed = true,
                    NegozioId = negozio1.Id
                },
                new ApplicationUser
                {
                    Id = "user-n2",
                    UserName = "user2@example.com",
                    NormalizedUserName = "USER2@EXAMPLE.COM",
                    Email = "user2@example.com",
                    NormalizedEmail = "USER2@EXAMPLE.COM",
                    EmailConfirmed = true,
                    NegozioId = negozio2.Id
                });

            var cliente1 = new Cliente
            {
                Nome = "Anna",
                Cognome = "Uno",
                Email = "anna.uno@example.com",
                Paese = "Italy",
                NegozioId = negozio1.Id
            };
            var cliente2 = new Cliente
            {
                Nome = "Bruno",
                Cognome = "Due",
                Email = "bruno.due@example.com",
                Paese = "Italy",
                NegozioId = negozio2.Id
            };

            seedContext.Clienti.AddRange(cliente1, cliente2);
            await seedContext.SaveChangesAsync();

            seedContext.Misure.AddRange(
                new MisureCliente { ClienteId = cliente1.Id, TipoMisura = "Giacca", RecordId = 11 },
                new MisureCliente { ClienteId = cliente2.Id, TipoMisura = "Camicia", RecordId = 22 });

            await seedContext.SaveChangesAsync();
            negozio1Id = negozio1.Id;
        }

        mockTenant.Setup(s => s.GetCurrentNegozioId()).Returns(negozio1Id);

        using (var actContext = factory.CreateContext())
        {
            var service = new DashboardService(CreateScopeFactory(factory), new MemoryCache(new MemoryCacheOptions()));
            var result = await service.GetKpiAsync(negozio1Id, isAdmin: false);

            Assert.Equal(1, result.TotalClients);
            Assert.Equal(1, result.TotalStores);
            Assert.Equal(1, result.TotalStaff);
            Assert.Equal(1, result.TotalMeasurements);
        }
    }

    [Fact]
    public async Task GetKpiAsync_NonAdminWithoutNegozio_ReturnsEmptySnapshot()
    {
        using var factory = new TestDbContextFactory();
        var mockTenant = new Mock<Services.ITenantService>();
        mockTenant.Setup(s => s.IsAdmin()).Returns(false);
        mockTenant.Setup(s => s.GetCurrentNegozioId()).Returns((int?)null);

        using var context = factory.CreateContext();

        var service = new DashboardService(CreateScopeFactory(factory), new MemoryCache(new MemoryCacheOptions()));
        var result = await service.GetKpiAsync(negozioId: null, isAdmin: false);

        Assert.Equal(0, result.TotalClients);
        Assert.Equal(0, result.TotalStores);
        Assert.Equal(0, result.TotalStaff);
        Assert.Equal(0, result.TotalMeasurements);
    }
}
