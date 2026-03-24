using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MisureRicci.Models;
using MisureRicci.Services;
using Xunit;

namespace MisureRicci.Tests;

public class LegacyMeasurementConverterTests
{
    [Fact]
    public async Task ConvertAsync_GiaccaLegacy_CreatesRecordValuesAndDynamicRegistryEntry()
    {
        using var factory = new TestDbContextFactory();

        int clienteId;
        int giacca1Id;
        int giaccaTypeId;

        using (var seedCtx = factory.CreateContext())
        {
            var adminRole = new IdentityRole
            {
                Id = "role-admin",
                Name = "Admin",
                NormalizedName = "ADMIN"
            };
            seedCtx.Roles.Add(adminRole);

            seedCtx.Users.Add(new ApplicationUser
            {
                Id = "convert-user",
                UserName = "convert@test.com",
                NormalizedUserName = "CONVERT@TEST.COM",
                Email = "convert@test.com",
                NormalizedEmail = "CONVERT@TEST.COM",
                EmailConfirmed = true
            });
            seedCtx.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = "convert-user",
                RoleId = adminRole.Id
            });

            var cliente = new Cliente
            {
                Nome = "Marco",
                Cognome = "Ferrari",
                Email = "marco.ferrari@example.com",
                Paese = "Italy"
            };
            seedCtx.Clienti.Add(cliente);
            await seedCtx.SaveChangesAsync();

            var giacca = new GiaccaMeasurement
            {
                ClienteId = cliente.Id,
                Spalle = 46.0,
                Torace = 100.0,
                Vita = 90.0,
                Manica = 64.0,
                Lunghezza = 77.0
            };
            seedCtx.MisureGiacca.Add(giacca);
            await seedCtx.SaveChangesAsync();

            // Seed the matching MeasurementType with field definitions
            var giaccaType = new MeasurementType { Nome = "Giacca", IsActive = true };
            seedCtx.DynamicMeasurementTypes.Add(giaccaType);
            await seedCtx.SaveChangesAsync();

            seedCtx.DynamicFieldDefinitions.AddRange(
                new MeasurementFieldDefinition { MeasurementTypeId = giaccaType.Id, NomeCampo = "Spalle",    Etichetta = "Spalle",    TipoDato = DynamicFieldType.Decimal, IsActive = true },
                new MeasurementFieldDefinition { MeasurementTypeId = giaccaType.Id, NomeCampo = "Torace",    Etichetta = "Torace",    TipoDato = DynamicFieldType.Decimal, IsActive = true },
                new MeasurementFieldDefinition { MeasurementTypeId = giaccaType.Id, NomeCampo = "Vita",      Etichetta = "Vita",      TipoDato = DynamicFieldType.Decimal, IsActive = true },
                new MeasurementFieldDefinition { MeasurementTypeId = giaccaType.Id, NomeCampo = "Manica",    Etichetta = "Manica",    TipoDato = DynamicFieldType.Decimal, IsActive = true },
                new MeasurementFieldDefinition { MeasurementTypeId = giaccaType.Id, NomeCampo = "Lunghezza", Etichetta = "Lunghezza", TipoDato = DynamicFieldType.Decimal, IsActive = true }
            );
            await seedCtx.SaveChangesAsync();

            clienteId = cliente.Id;
            giacca1Id = giacca.Id;
            giaccaTypeId = giaccaType.Id;
        }

        using (var actCtx = factory.CreateContext())
        {
            var legacy = await actCtx.MisureGiacca.FindAsync(giacca1Id);
            Assert.NotNull(legacy);

            var converter = new LegacyMeasurementConverter(actCtx);
            var record = await converter.ConvertAsync(legacy!, "Giacca", "convert-user");

            Assert.NotNull(record);
            Assert.NotEqual(0, record!.Id);
        }

        using (var assertCtx = factory.CreateContext())
        {
            var record = await assertCtx.DynamicMeasurementRecords.SingleAsync();
            Assert.Equal(clienteId, record.ClienteId);
            Assert.Equal(giaccaTypeId, record.MeasurementTypeId);
            Assert.Equal("convert-user", record.CreatedByUserId);

            var values = await assertCtx.DynamicMeasurementValues.ToListAsync();
            Assert.Equal(5, values.Count);
            Assert.Contains(values, v => v.Valore == "46");

            var dynRegistry = await assertCtx.Misure.SingleAsync(r => r.IsDynamic);
            Assert.Equal(record.Id, dynRegistry.RecordId);
            Assert.Contains($"legacy: {giacca1Id}", dynRegistry.SystemNote);
        }
    }

    [Fact]
    public async Task ConvertAsync_UnknownType_ReturnsNull()
    {
        using var factory = new TestDbContextFactory();

        int clienteId;
        int giacca1Id;

        using (var seedCtx = factory.CreateContext())
        {
            var adminRole = new IdentityRole
            {
                Id = "role-admin-2",
                Name = "Admin",
                NormalizedName = "ADMIN"
            };
            seedCtx.Roles.Add(adminRole);

            seedCtx.Users.Add(new ApplicationUser
            {
                Id = "u2",
                UserName = "u2@test.com",
                NormalizedUserName = "U2@TEST.COM",
                Email = "u2@test.com",
                NormalizedEmail = "U2@TEST.COM",
                EmailConfirmed = true
            });
            seedCtx.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = "u2",
                RoleId = adminRole.Id
            });

            var cliente = new Cliente { Nome = "A", Cognome = "B", Email = "a.b@example.com", Paese = "Italy" };
            seedCtx.Clienti.Add(cliente);
            await seedCtx.SaveChangesAsync();

            var giacca = new GiaccaMeasurement { ClienteId = cliente.Id, Spalle = 44 };
            seedCtx.MisureGiacca.Add(giacca);
            await seedCtx.SaveChangesAsync();

            clienteId = cliente.Id;
            giacca1Id = giacca.Id;
        }

        using var actCtx = factory.CreateContext();
        var legacy = await actCtx.MisureGiacca.FindAsync(giacca1Id);
        var converter = new LegacyMeasurementConverter(actCtx);

        var result = await converter.ConvertAsync(legacy!, "TipoInesistente", "u2");

        Assert.Null(result);
    }
}
