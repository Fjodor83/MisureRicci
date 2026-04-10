using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Moq;
using MisureRicci.Models;
using MisureRicci.Services;
using Xunit;

namespace MisureRicci.Tests.Unit.Services;

public class ClienteServiceTests
{
    [Fact]
    public async Task TestGenerateClientCode_Format_SR_YYYY_NNNNN()
    {
        using var factory = new TestDbContextFactory();
        int negozioId;

        using (var seedContext = factory.CreateContext())
        {
            var negozio = new Negozio
            {
                Nome = "Boutique Centro",
                Citta = "Torino",
                Paese = "Italy"
            };
            seedContext.Negozi.Add(negozio);
            await seedContext.SaveChangesAsync();
            negozioId = negozio.Id;
        }

        using var context = factory.CreateContext();
        var service = new ClienteService(context, new Mock<IAuditService>().Object, new Mock<IHttpContextAccessor>().Object);

        var result = await service.CreateClienteScopedAsync(new Cliente
        {
            Nome = "Mario",
            Cognome = "Rossi",
            Email = "mario.rossi@example.com",
            Paese = "Italy",
            NegozioId = negozioId
        }, negozioId: null, isAdmin: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ClientCode.Should().MatchRegex("^SR-\\d{4}-\\d{5}$");
    }

    [Fact]
    public async Task TestCreateCliente_NonAdmin_ForcesNegozioId()
    {
        using var factory = new TestDbContextFactory();

        int negozioId;
        using (var seedContext = factory.CreateContext())
        {
            var negozio = new Negozio
            {
                Nome = "Boutique Test",
                Citta = "Milano",
                Paese = "Italy"
            };
            seedContext.Negozi.Add(negozio);
            await seedContext.SaveChangesAsync();
            negozioId = negozio.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var service = new ClienteService(actContext, new Mock<IAuditService>().Object, new Mock<IHttpContextAccessor>().Object);

            var model = new Cliente
            {
                Nome = "Laura",
                Cognome = "Bianchi",
                Email = "laura.bianchi@example.com",
                Paese = "Italy",
                NegozioId = negozioId + 999
            };

            var result = await service.CreateClienteScopedAsync(model, negozioId, isAdmin: false);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.NegozioId.Should().Be(negozioId);

            var persisted = await actContext.Clienti.AsNoTracking().SingleAsync(c => c.Id == result.Value.Id);
            persisted.NegozioId.Should().Be(negozioId);
        }
    }
}
