using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using MisureRicci.Services;
using Xunit;

namespace MisureRicci.Tests;

public class MeasurementServiceTests
{
    // ─── seed helpers ────────────────────────────────────────────────────────

    private static async Task<(int clienteId, int giacca1Id, int registryId, int negozioId)> SeedGiaccaAsync(
        TestDbContextFactory factory)
    {
        using var ctx = factory.CreateContext();

        ctx.Users.Add(new ApplicationUser
        {
            Id = "seed-user",
            UserName = "seed-user@test.com",
            NormalizedUserName = "SEED-USER@TEST.COM",
            Email = "seed-user@test.com",
            NormalizedEmail = "SEED-USER@TEST.COM",
            EmailConfirmed = true
        });

        var negozio = new Negozio { Nome = "Boutique Test", Citta = "Milano", Paese = "Italy" };
        ctx.Negozi.Add(negozio);
        await ctx.SaveChangesAsync();

        var cliente = new Cliente
        {
            Nome = "Test",
            Cognome = "Cliente",
            Email = "test@example.com",
            Paese = "Italy",
            NegozioId = negozio.Id
        };
        ctx.Clienti.Add(cliente);
        await ctx.SaveChangesAsync();

        var giacca = new GiaccaMeasurement
        {
            ClienteId = cliente.Id,
            Spalle = 45.0,
            Torace = 98.0,
            Vita = 88.0,
            Manica = 63.0,
            Lunghezza = 76.0
        };
        ctx.MisureGiacca.Add(giacca);
        await ctx.SaveChangesAsync();

        var registro = new MisureCliente
        {
            ClienteId = cliente.Id,
            TipoMisura = "Giacca",
            RecordId = giacca.Id
        };
        ctx.Misure.Add(registro);
        await ctx.SaveChangesAsync();

        return (cliente.Id, giacca.Id, registro.Id, negozio.Id);
    }

    // ─── GetMeasurementScopedAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetMeasurementScopedAsync_NonAdmin_CorrectNegozioId_ReturnsMeasurement()
    {
        using var factory = new TestDbContextFactory();
        var (_, giacca1Id, _, negozioId) = await SeedGiaccaAsync(factory);

        using var ctx = factory.CreateContext();
        var service = new MeasurementService(ctx);

        var result = await service.GetMeasurementScopedAsync(giacca1Id, "giacca", negozioId: negozioId, isAdmin: false);

        Assert.NotNull(result);
        Assert.IsType<GiaccaMeasurement>(result);
    }

    [Fact]
    public async Task GetMeasurementScopedAsync_NonAdmin_WrongNegozioId_ReturnsNull()
    {
        using var factory = new TestDbContextFactory();
        var (_, giacca1Id, _, negozioId) = await SeedGiaccaAsync(factory);

        using var ctx = factory.CreateContext();
        var service = new MeasurementService(ctx);

        var result = await service.GetMeasurementScopedAsync(giacca1Id, "giacca", negozioId: negozioId + 999, isAdmin: false);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMeasurementScopedAsync_IsAdmin_AnyNegozioId_ReturnsMeasurement()
    {
        using var factory = new TestDbContextFactory();
        var (_, giacca1Id, _, negozioId) = await SeedGiaccaAsync(factory);

        using var ctx = factory.CreateContext();
        var service = new MeasurementService(ctx);

        var result = await service.GetMeasurementScopedAsync(giacca1Id, "giacca", negozioId: negozioId + 999, isAdmin: true);

        Assert.NotNull(result);
    }

    // ─── GetRegistryEntryAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetRegistryEntryAsync_NonAdmin_WrongNegozioId_ReturnsNull()
    {
        using var factory = new TestDbContextFactory();
        var (_, _, registryId, negozioId) = await SeedGiaccaAsync(factory);

        using var ctx = factory.CreateContext();
        var service = new MeasurementService(ctx);

        var result = await service.GetRegistryEntryAsync(registryId, negozioId: negozioId + 999, isAdmin: false);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRegistryEntryAsync_IsAdmin_ReturnEntryRegardlessOfNegozio()
    {
        using var factory = new TestDbContextFactory();
        var (_, _, registryId, negozioId) = await SeedGiaccaAsync(factory);

        using var ctx = factory.CreateContext();
        var service = new MeasurementService(ctx);

        var result = await service.GetRegistryEntryAsync(registryId, negozioId: negozioId + 999, isAdmin: true);

        Assert.NotNull(result);
        Assert.Equal("Giacca", result!.TipoMisura);
    }

    // ─── UpdateMeasurementAsync ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateMeasurementAsync_Giacca_PersistsChangedFields()
    {
        using var factory = new TestDbContextFactory();
        var (_, giacca1Id, _, _) = await SeedGiaccaAsync(factory);

        using (var ctx = factory.CreateContext())
        {
            var service = new MeasurementService(ctx);
            var giacca = (GiaccaMeasurement)(await service.GetMeasurementAsync(giacca1Id, "giacca"))!;
            giacca.Spalle = 47.5;
            await service.UpdateMeasurementAsync(giacca, "giacca");
        }

        using var assertCtx = factory.CreateContext();
        var updated = await assertCtx.MisureGiacca.FindAsync(giacca1Id);
        Assert.NotNull(updated);
        Assert.Equal(47.5, updated!.Spalle);
    }

    // ─── DeleteByRegistryEntryAsync ───────────────────────────────────────────

    [Fact]
    public async Task DeleteByRegistryEntryAsync_RemovesMeasurementAndRegistryEntry()
    {
        using var factory = new TestDbContextFactory();
        var (clienteId, giacca1Id, registryId, negozioId) = await SeedGiaccaAsync(factory);

        using (var ctx = factory.CreateContext())
        {
            var service = new MeasurementService(ctx);
            var returnedClienteId = await service.DeleteByRegistryEntryAsync(registryId, negozioId: negozioId, isAdmin: false);
            Assert.Equal(clienteId, returnedClienteId);
        }

        using var assertCtx = factory.CreateContext();
        Assert.Null(await assertCtx.MisureGiacca.FindAsync(giacca1Id));
        Assert.Null(await assertCtx.Misure.FindAsync(registryId));
    }
}
