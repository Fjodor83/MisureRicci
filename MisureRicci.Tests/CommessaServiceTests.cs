using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using Xunit;

namespace MisureRicci.Tests;

public class CommessaServiceTests
{
    [Fact]
    public async Task CreateCommessaAsync_CreatesOpeningEventAndSelectedMeasurementLinks()
    {
        using var factory = new TestDbContextFactory();

        int clienteId;
        int misuraId;

        using (var seedContext = factory.CreateContext())
        {
            seedContext.Users.Add(new ApplicationUser
            {
                Id = "user-1",
                UserName = "user-1@example.com",
                NormalizedUserName = "USER-1@EXAMPLE.COM",
                Email = "user-1@example.com",
                NormalizedEmail = "USER-1@EXAMPLE.COM",
                EmailConfirmed = true
            });

            var cliente = new Cliente
            {
                Nome = "Mario",
                Cognome = "Rossi",
                Email = "mario.rossi@example.com",
                Paese = "Italy"
            };

            seedContext.Clienti.Add(cliente);
            await seedContext.SaveChangesAsync();

            var misura = new MisureCliente
            {
                ClienteId = cliente.Id,
                TipoMisura = "Giacca",
                RecordId = 101,
                Note = "Misura seed"
            };

            seedContext.Misure.Add(misura);
            await seedContext.SaveChangesAsync();

            clienteId = cliente.Id;
            misuraId = misura.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var service = new CommessaService(actContext, new MemoryCache(new MemoryCacheOptions()), NullLogger<CommessaService>.Instance);
            var model = new CommessaCreateViewModel
            {
                ClienteId = clienteId,
                TipoCapo = "Giacca monopetto",
                Tessuto = "Lana",
                SelectedMisuraIds = new List<int> { misuraId }
            };

            var created = await service.CreateCommessaAsync(model, "user-1", negozioId: null, isAdmin: true);

            Assert.True(created.IsSuccess);
            Assert.NotNull(created.Value);
            Assert.NotEqual(0, created.Value!.Id);
            Assert.Equal($"CM-{DateTime.UtcNow.Year}-{created.Value.Id:D6}", created.Value.CommessaCode);
        }

        using (var assertContext = factory.CreateContext())
        {
            var commessa = await assertContext.Commissioni.SingleAsync();
            var eventi = await assertContext.CommissioniEventi
                .Where(x => x.CommessaSartorialeId == commessa.Id)
                .OrderBy(x => x.Id)
                .ToListAsync();
            var links = await assertContext.CommissioniMisureLinks
                .Where(x => x.CommessaSartorialeId == commessa.Id)
                .ToListAsync();

            Assert.Equal(StatoCommessa.MisureRaccolte, commessa.Stato);
            Assert.Equal(2, eventi.Count);
            Assert.Contains(eventi, x => x.TipoEvento == "Apertura" && x.NuovoStato == StatoCommessa.MisureRaccolte);
            Assert.Contains(eventi, x => x.TipoEvento == "LinkMisura" && x.CreatedByUserId == "user-1");
            Assert.Single(links);
            Assert.Equal(misuraId, links[0].MisuraClienteId);
            Assert.Equal("user-1", links[0].LinkedByUserId);
        }
    }

    [Fact]
    public async Task CreateCommessaAsync_WithMeasurementFromAnotherCliente_Fails()
    {
        using var factory = new TestDbContextFactory();

        int clienteId;
        int misuraAltroClienteId;

        using (var seedContext = factory.CreateContext())
        {
            var cliente = new Cliente
            {
                Nome = "Cliente",
                Cognome = "Uno",
                Email = "cliente1@example.com",
                Paese = "Italy"
            };
            var altroCliente = new Cliente
            {
                Nome = "Cliente",
                Cognome = "Due",
                Email = "cliente2@example.com",
                Paese = "Italy"
            };

            seedContext.Clienti.AddRange(cliente, altroCliente);
            await seedContext.SaveChangesAsync();

            var misuraAltroCliente = new MisureCliente
            {
                ClienteId = altroCliente.Id,
                TipoMisura = "Giacca",
                RecordId = 202
            };

            seedContext.Misure.Add(misuraAltroCliente);
            await seedContext.SaveChangesAsync();

            clienteId = cliente.Id;
            misuraAltroClienteId = misuraAltroCliente.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var service = new CommessaService(actContext, new MemoryCache(new MemoryCacheOptions()), NullLogger<CommessaService>.Instance);
            var model = new CommessaCreateViewModel
            {
                ClienteId = clienteId,
                TipoCapo = "Abito",
                SelectedMisuraIds = new List<int> { misuraAltroClienteId }
            };

            var result = await service.CreateCommessaAsync(model, "user-1", negozioId: null, isAdmin: true);

            Assert.False(result.IsSuccess);
            Assert.Equal("Una o più misure selezionate non sono valide per il cliente.", result.Error);
        }

        using (var assertContext = factory.CreateContext())
        {
            Assert.Empty(await assertContext.Commissioni.ToListAsync());
        }
    }
}
