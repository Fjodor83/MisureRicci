using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using Xunit;

namespace MisureRicci.Tests.Unit.Services;

public class CommessaServiceTests
{
    private static ICommessaService BuildService(ApplicationDbContext context)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        ICommessaMisuraLinkService linkService = new CommessaMisuraLinkService(context, NullLogger<CommessaMisuraLinkService>.Instance);
        ICommessaQueryService queryService = new CommessaQueryService(context, cache);
        ICommessaCommandService commandService = new CommessaCommandService(context, NullLogger<CommessaCommandService>.Instance, linkService);
        return new CommessaService(queryService, commandService, linkService);
    }

    [Fact]
    public async Task TestAdvanceStato_FromBozza_ToMisureRaccolte_Succeeds()
    {
        using var factory = new TestDbContextFactory();

        int commessaId;
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

            var negozio = new Negozio
            {
                Nome = "Boutique Milano",
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

            var commessa = new CommessaSartoriale
            {
                ClienteId = cliente.Id,
                NegozioId = negozio.Id,
                TipoCapo = "Giacca",
                Stato = StatoCommessa.Bozza,
                DataApertura = DateTime.UtcNow
            };

            var misura = new MisureCliente
            {
                ClienteId = cliente.Id,
                TipoMisura = "Giacca",
                RecordId = 123,
                DataCreazione = DateTime.UtcNow
            };

            seedContext.Commissioni.Add(commessa);
            seedContext.Misure.Add(misura);
            await seedContext.SaveChangesAsync();

            seedContext.CommissioniMisureLinks.Add(new CommessaMisuraLink
            {
                CommessaSartorialeId = commessa.Id,
                MisuraClienteId = misura.Id,
                LinkedAt = DateTime.UtcNow,
                LinkedByUserId = "user-1"
            });
            await seedContext.SaveChangesAsync();

            commessaId = commessa.Id;
            misuraId = misura.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var service = BuildService(actContext);
            var result = await service.AdvanceStatoAsync(
                commessaId,
                StatoCommessa.MisureRaccolte,
                "ok",
                "user-1",
                negozioId: null,
                isAdmin: true);

            result.IsSuccess.Should().BeTrue();
        }

        using (var assertContext = factory.CreateContext())
        {
            var commessa = await assertContext.Commissioni.SingleAsync(c => c.Id == commessaId);
            commessa.Stato.Should().Be(StatoCommessa.MisureRaccolte);

            var hasEvento = await assertContext.CommissioniEventi.AnyAsync(e =>
                e.CommessaSartorialeId == commessaId
                && e.TipoEvento == "CambioStato"
                && e.NuovoStato == StatoCommessa.MisureRaccolte);
            hasEvento.Should().BeTrue();

            var hasLink = await assertContext.CommissioniMisureLinks.AnyAsync(l =>
                l.CommessaSartorialeId == commessaId
                && l.MisuraClienteId == misuraId);
            hasLink.Should().BeTrue();
        }
    }

    [Fact]
    public async Task TestAdvanceStato_InvalidTransition_ReturnsFail()
    {
        using var factory = new TestDbContextFactory();

        int commessaId;
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

            var negozio = new Negozio
            {
                Nome = "Boutique Roma",
                Citta = "Roma",
                Paese = "Italy"
            };
            seedContext.Negozi.Add(negozio);
            await seedContext.SaveChangesAsync();

            var cliente = new Cliente
            {
                Nome = "Luigi",
                Cognome = "Verdi",
                Email = "luigi.verdi@example.com",
                Paese = "Italy",
                NegozioId = negozio.Id
            };
            seedContext.Clienti.Add(cliente);
            await seedContext.SaveChangesAsync();

            var commessa = new CommessaSartoriale
            {
                ClienteId = cliente.Id,
                NegozioId = negozio.Id,
                TipoCapo = "Abito",
                Stato = StatoCommessa.Bozza,
                DataApertura = DateTime.UtcNow
            };
            seedContext.Commissioni.Add(commessa);
            await seedContext.SaveChangesAsync();
            commessaId = commessa.Id;
        }

        using var actContext = factory.CreateContext();
        var service = BuildService(actContext);

        var result = await service.AdvanceStatoAsync(
            commessaId,
            StatoCommessa.Consegnata,
            null,
            "user-1",
            negozioId: null,
            isAdmin: true);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("non consentita");
    }

    [Fact]
    public async Task TestAdvanceStato_RequiresMisura_WhenNoMisuraLinked_ReturnsFail()
    {
        using var factory = new TestDbContextFactory();

        int commessaId;
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

            var negozio = new Negozio
            {
                Nome = "Boutique Torino",
                Citta = "Torino",
                Paese = "Italy"
            };
            seedContext.Negozi.Add(negozio);
            await seedContext.SaveChangesAsync();

            var cliente = new Cliente
            {
                Nome = "Paolo",
                Cognome = "Neri",
                Email = "paolo.neri@example.com",
                Paese = "Italy",
                NegozioId = negozio.Id
            };
            seedContext.Clienti.Add(cliente);
            await seedContext.SaveChangesAsync();

            var commessa = new CommessaSartoriale
            {
                ClienteId = cliente.Id,
                NegozioId = negozio.Id,
                TipoCapo = "Pantalone",
                Stato = StatoCommessa.Bozza,
                DataApertura = DateTime.UtcNow
            };
            seedContext.Commissioni.Add(commessa);
            await seedContext.SaveChangesAsync();
            commessaId = commessa.Id;
        }

        using var actContext = factory.CreateContext();
        var service = BuildService(actContext);

        var result = await service.AdvanceStatoAsync(
            commessaId,
            StatoCommessa.MisureRaccolte,
            null,
            "user-1",
            negozioId: null,
            isAdmin: true);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Almeno una misura collegata");
    }

    [Fact]
    public async Task TestCreateCommessa_WithValidData_ReturnsOk()
    {
        using var factory = new TestDbContextFactory();

        int clienteId;
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

            var negozio = new Negozio
            {
                Nome = "Boutique Firenze",
                Citta = "Firenze",
                Paese = "Italy"
            };
            seedContext.Negozi.Add(negozio);
            await seedContext.SaveChangesAsync();

            var cliente = new Cliente
            {
                Nome = "Anna",
                Cognome = "Bianchi",
                Email = "anna.bianchi@example.com",
                Paese = "Italy",
                NegozioId = negozio.Id
            };

            seedContext.Clienti.Add(cliente);
            await seedContext.SaveChangesAsync();
            clienteId = cliente.Id;
        }

        using var actContext = factory.CreateContext();
        var service = BuildService(actContext);

        var model = new CommessaCreateViewModel
        {
            ClienteId = clienteId,
            TipoCapo = "Giacca doppiopetto",
            Tessuto = "Lana",
            SelectedMisuraIds = new List<int>()
        };

        var result = await service.CreateCommessaAsync(model, "user-1", negozioId: null, isAdmin: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TipoCapo.Should().Be("Giacca doppiopetto");
        result.Value.CommessaCode.Should().MatchRegex("^CM-\\d{4}-\\d{6}$");
    }
}
