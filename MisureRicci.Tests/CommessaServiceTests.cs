using Microsoft.EntityFrameworkCore;
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

            seedContext.RegistroMisure.Add(misura);
            await seedContext.SaveChangesAsync();

            clienteId = cliente.Id;
            misuraId = misura.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var service = new CommessaService(actContext);
            var model = new CommessaCreateViewModel
            {
                ClienteId = clienteId,
                TipoCapo = "Giacca monopetto",
                Tessuto = "Lana",
                SelectedMisuraIds = new List<int> { misuraId }
            };

            var created = await service.CreateCommessaAsync(model, "user-1");

            Assert.NotEqual(0, created.Id);
            Assert.Equal($"CM-{DateTime.UtcNow.Year}-{created.Id:D6}", created.CommessaCode);
        }

        using (var assertContext = factory.CreateContext())
        {
            var commessa = await assertContext.CommissioniSartoriali.SingleAsync();
            var eventi = await assertContext.CommissioniEventi
                .Where(x => x.CommessaSartorialeId == commessa.Id)
                .OrderBy(x => x.Id)
                .ToListAsync();
            var links = await assertContext.CommissioniMisureLinks
                .Where(x => x.CommessaSartorialeId == commessa.Id)
                .ToListAsync();

            Assert.Equal(2, eventi.Count);
            Assert.Contains(eventi, x => x.TipoEvento == "Apertura" && x.NuovoStato == StatoCommessa.Bozza);
            Assert.Contains(eventi, x => x.TipoEvento == "LinkMisura" && x.CreatedByUserId == "user-1");
            Assert.Single(links);
            Assert.Equal(misuraId, links[0].MisuraClienteId);
            Assert.Equal("user-1", links[0].LinkedByUserId);
        }
    }
}