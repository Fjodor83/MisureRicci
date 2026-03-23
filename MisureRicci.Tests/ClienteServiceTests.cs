using MisureRicci.Models;
using MisureRicci.Services;
using Xunit;

namespace MisureRicci.Tests;

public class ClienteServiceTests
{
    [Fact]
    public async Task GetClienteScopedAsync_NonAdminWithoutNegozio_ReturnsNull()
    {
        using var factory = new TestDbContextFactory();

        int clienteId;
        using (var seedContext = factory.CreateContext())
        {
            var negozio = new Negozio
            {
                Nome = "Negozio Test",
                Citta = "Milano",
                Paese = "Italy"
            };
            seedContext.Negozi.Add(negozio);
            await seedContext.SaveChangesAsync();

            var cliente = new Cliente
            {
                Nome = "Mario",
                Cognome = "Rossi",
                Email = "mario.rossi@example.com",
                Paese = "Italy",
                NegozioId = negozio.Id
            };

            seedContext.Clienti.Add(cliente);
            await seedContext.SaveChangesAsync();
            clienteId = cliente.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var service = new ClienteService(actContext);
            var cliente = await service.GetClienteScopedAsync(clienteId, negozioId: null, isAdmin: false);

            Assert.Null(cliente);
        }
    }

    [Fact]
    public async Task CreateClienteScopedAsync_NonAdminAssignsNegozioAndNormalizesFields()
    {
        using var factory = new TestDbContextFactory();

        int negozioId;
        using (var seedContext = factory.CreateContext())
        {
            var negozio = new Negozio
            {
                Nome = "Negozio Test",
                Citta = "Roma",
                Paese = "Italy"
            };
            seedContext.Negozi.Add(negozio);
            await seedContext.SaveChangesAsync();
            negozioId = negozio.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var service = new ClienteService(actContext);
            var created = await service.CreateClienteScopedAsync(new Cliente
            {
                Nome = "  Anna  ",
                Cognome = "  Verdi ",
                Email = " anna.verdi@example.com ",
                Telefono = " 3331234567 ",
                Paese = " Italy "
            }, negozioId, isAdmin: false);

            Assert.NotNull(created);
            Assert.Equal(negozioId, created!.NegozioId);
            Assert.Equal("Anna", created.Nome);
            Assert.Equal("Verdi", created.Cognome);
            Assert.Equal("anna.verdi@example.com", created.Email);
            Assert.Equal("3331234567", created.Telefono);
            Assert.Equal("Italy", created.Paese);
        }
    }

    [Fact]
    public async Task CreateClienteScopedAsync_AdminWithoutNegozio_ReturnsNull()
    {
        using var factory = new TestDbContextFactory();

        using var context = factory.CreateContext();
        var service = new ClienteService(context);

        var created = await service.CreateClienteScopedAsync(new Cliente
        {
            Nome = "Laura",
            Cognome = "Bianchi",
            Email = "laura.bianchi@example.com",
            Paese = "Italy"
        }, negozioId: null, isAdmin: true);

        Assert.Null(created);
    }

    [Fact]
    public async Task CreateClienteScopedAsync_AdminWithNegozio_PreservesAssignment()
    {
        using var factory = new TestDbContextFactory();

        int negozioId;
        using (var seedContext = factory.CreateContext())
        {
            var negozio = new Negozio
            {
                Nome = "Boutique Centro",
                Citta = "Firenze",
                Paese = "Italy"
            };
            seedContext.Negozi.Add(negozio);
            await seedContext.SaveChangesAsync();
            negozioId = negozio.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var service = new ClienteService(actContext);
            var created = await service.CreateClienteScopedAsync(new Cliente
            {
                Nome = "Laura",
                Cognome = "Bianchi",
                Email = "laura.bianchi@example.com",
                Paese = "Italy",
                NegozioId = negozioId
            }, negozioId: null, isAdmin: true);

            Assert.NotNull(created);
            Assert.Equal(negozioId, created!.NegozioId);
        }
    }
}
