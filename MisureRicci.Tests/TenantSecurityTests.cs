using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using MisureRicci.Services;
using Xunit;

namespace MisureRicci.Tests;

public class TenantSecurityTests
{
    [Fact]
    public async Task PdfService_NonAdminWithMismatchedNegozio_ThrowsUnauthorizedAccessException()
    {
        using var factory = new TestDbContextFactory();

        int clienteId;
        using (var seedContext = factory.CreateContext())
        {
            var cliente = new Cliente
            {
                Nome = "Giulia",
                Cognome = "Neri",
                Email = "giulia.neri@example.com",
                Paese = "Italy",
                NegozioId = 10
            };

            seedContext.Clienti.Add(cliente);
            await seedContext.SaveChangesAsync();
            clienteId = cliente.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var pdfService = new PdfService(actContext);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                pdfService.GenerateDossierPdfAsync(clienteId, negozioId: 99, isAdmin: false));
        }
    }

    [Fact]
    public async Task MeasurementService_GetMeasurementScopedAsync_NonAdminWrongNegozio_ReturnsNull()
    {
        using var factory = new TestDbContextFactory();

        int measurementId;
        using (var seedContext = factory.CreateContext())
        {
            var cliente = new Cliente
            {
                Nome = "Paolo",
                Cognome = "Blu",
                Email = "paolo.blu@example.com",
                Paese = "Italy",
                NegozioId = 7
            };

            seedContext.Clienti.Add(cliente);
            await seedContext.SaveChangesAsync();

            var misura = new GiaccaMeasurement
            {
                ClienteId = cliente.Id,
                Spalle = 44,
                Torace = 100,
                Vita = 90,
                Manica = 63,
                Lunghezza = 72
            };

            seedContext.MisureGiacca.Add(misura);
            await seedContext.SaveChangesAsync();
            measurementId = misura.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var measurementService = new MeasurementService(actContext);
            var result = await measurementService.GetMeasurementScopedAsync(measurementId, "giacca", negozioId: 99, isAdmin: false);

            Assert.Null(result);
        }
    }

    [Fact]
    public async Task MeasurementService_GetMeasurementScopedAsync_AdminBypassesNegozioFilter()
    {
        using var factory = new TestDbContextFactory();

        int measurementId;
        using (var seedContext = factory.CreateContext())
        {
            var cliente = new Cliente
            {
                Nome = "Luca",
                Cognome = "Viola",
                Email = "luca.viola@example.com",
                Paese = "Italy",
                NegozioId = 3
            };

            seedContext.Clienti.Add(cliente);
            await seedContext.SaveChangesAsync();

            var misura = new GiaccaMeasurement
            {
                ClienteId = cliente.Id,
                Spalle = 45,
                Torace = 102,
                Vita = 92,
                Manica = 64,
                Lunghezza = 73
            };

            seedContext.MisureGiacca.Add(misura);
            await seedContext.SaveChangesAsync();
            measurementId = misura.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var measurementService = new MeasurementService(actContext);
            var result = await measurementService.GetMeasurementScopedAsync(measurementId, "giacca", negozioId: null, isAdmin: true);

            Assert.NotNull(result);
            Assert.IsType<GiaccaMeasurement>(result);
        }
    }

    [Fact]
    public async Task MeasurementService_GetGlobalRegistryAsync_NonAdminReturnsOnlyOwnNegozio()
    {
        using var factory = new TestDbContextFactory();

        using (var seedContext = factory.CreateContext())
        {
            var clienteA = new Cliente
            {
                Nome = "Anna",
                Cognome = "A",
                Email = "anna.a@example.com",
                Paese = "Italy",
                NegozioId = 1
            };

            var clienteB = new Cliente
            {
                Nome = "Bruno",
                Cognome = "B",
                Email = "bruno.b@example.com",
                Paese = "Italy",
                NegozioId = 2
            };

            seedContext.Clienti.AddRange(clienteA, clienteB);
            await seedContext.SaveChangesAsync();

            seedContext.RegistroMisure.AddRange(
                new MisureCliente { ClienteId = clienteA.Id, TipoMisura = "Giacca", RecordId = 1, Note = "A" },
                new MisureCliente { ClienteId = clienteB.Id, TipoMisura = "Giacca", RecordId = 2, Note = "B" }
            );
            await seedContext.SaveChangesAsync();
        }

        using (var actContext = factory.CreateContext())
        {
            var measurementService = new MeasurementService(actContext);
            var result = await measurementService.GetGlobalRegistryAsync(filter: string.Empty, negozioId: 1, isAdmin: false);

            Assert.Single(result);
            Assert.All(result, item => Assert.Equal(1, item.Cliente?.NegozioId));
        }
    }
}