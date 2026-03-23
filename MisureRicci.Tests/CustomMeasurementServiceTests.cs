using Microsoft.EntityFrameworkCore;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using MisureRicci.Services;
using Xunit;

namespace MisureRicci.Tests;

public class CustomMeasurementServiceTests
{
    [Fact]
    public async Task CreateDynamicMeasurementAsync_CreatesRecordValuesAndRegistryEntry()
    {
        using var factory = new TestDbContextFactory();

        int clienteId;
        int typeId;
        int fieldId;

        using (var seedContext = factory.CreateContext())
        {
            seedContext.Users.Add(new ApplicationUser
            {
                Id = "creator-1",
                UserName = "creator-1@example.com",
                NormalizedUserName = "CREATOR-1@EXAMPLE.COM",
                Email = "creator-1@example.com",
                NormalizedEmail = "CREATOR-1@EXAMPLE.COM",
                EmailConfirmed = true
            });

            var cliente = new Cliente
            {
                Nome = "Luigi",
                Cognome = "Bianchi",
                Email = "luigi.bianchi@example.com",
                Paese = "Italy"
            };

            var type = new MeasurementType
            {
                Nome = "Soprabito",
                IsActive = true
            };

            seedContext.Clienti.Add(cliente);
            seedContext.MeasurementTypes.Add(type);
            await seedContext.SaveChangesAsync();

            var field = new MeasurementFieldDefinition
            {
                MeasurementTypeId = type.Id,
                NomeCampo = "Torace",
                Etichetta = "Torace",
                TipoDato = DynamicFieldType.Decimal,
                Obbligatorio = true,
                IsActive = true
            };

            seedContext.MeasurementFieldDefinitions.Add(field);
            await seedContext.SaveChangesAsync();

            clienteId = cliente.Id;
            typeId = type.Id;
            fieldId = field.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var service = new CustomMeasurementService(actContext);
            var model = new DynamicMeasurementCreateViewModel
            {
                ClienteId = clienteId,
                MeasurementTypeId = typeId,
                Fields = new List<DynamicFieldInputViewModel>
                {
                    new()
                    {
                        FieldDefinitionId = fieldId,
                        NomeCampo = "Torace",
                        Etichetta = "Torace",
                        TipoDato = DynamicFieldType.Decimal,
                        Template = DynamicFieldTemplate.Standard,
                        Valore = "51.5"
                    }
                }
            };

            var record = await service.CreateDynamicMeasurementAsync(model, "creator-1");
            Assert.NotEqual(0, record.Id);
        }

        using (var assertContext = factory.CreateContext())
        {
            var record = await assertContext.DynamicMeasurementRecords.SingleAsync();
            var value = await assertContext.DynamicMeasurementValues.SingleAsync();
            var registro = await assertContext.RegistroMisure.SingleAsync();

            Assert.Equal(clienteId, record.ClienteId);
            Assert.Equal(typeId, record.MeasurementTypeId);
            Assert.Equal("creator-1", record.CreatedByUserId);
            Assert.Equal(record.Id, value.DynamicMeasurementRecordId);
            Assert.Equal(fieldId, value.MeasurementFieldDefinitionId);
            Assert.Equal("51.5", value.Valore);
            Assert.True(registro.IsDynamic);
            Assert.Equal(record.Id, registro.RecordId);
            Assert.Equal("Soprabito", registro.TipoMisura);
        }
    }

    [Fact]
    public async Task UpdateDynamicMeasurementAsync_ReplacesExistingValuesAtomically()
    {
        using var factory = new TestDbContextFactory();

        int recordId;
        int typeId;
        int fieldId;

        using (var seedContext = factory.CreateContext())
        {
            var cliente = new Cliente
            {
                Nome = "Anna",
                Cognome = "Verdi",
                Email = "anna.verdi@example.com",
                Paese = "Italy"
            };

            var type = new MeasurementType
            {
                Nome = "Cappotto",
                IsActive = true
            };

            seedContext.Clienti.Add(cliente);
            seedContext.MeasurementTypes.Add(type);
            await seedContext.SaveChangesAsync();

            var field = new MeasurementFieldDefinition
            {
                MeasurementTypeId = type.Id,
                NomeCampo = "Lunghezza",
                Etichetta = "Lunghezza",
                TipoDato = DynamicFieldType.Decimal,
                Obbligatorio = true,
                IsActive = true
            };

            seedContext.MeasurementFieldDefinitions.Add(field);
            await seedContext.SaveChangesAsync();

            var record = new DynamicMeasurementRecord
            {
                ClienteId = cliente.Id,
                MeasurementTypeId = type.Id,
                CreatedAt = DateTime.UtcNow
            };

            seedContext.DynamicMeasurementRecords.Add(record);
            await seedContext.SaveChangesAsync();

            seedContext.DynamicMeasurementValues.Add(new DynamicMeasurementValue
            {
                DynamicMeasurementRecordId = record.Id,
                MeasurementFieldDefinitionId = field.Id,
                Valore = "100"
            });

            await seedContext.SaveChangesAsync();

            recordId = record.Id;
            typeId = type.Id;
            fieldId = field.Id;
        }

        using (var actContext = factory.CreateContext())
        {
            var service = new CustomMeasurementService(actContext);
            var model = new DynamicMeasurementCreateViewModel
            {
                RecordId = recordId,
                MeasurementTypeId = typeId,
                Fields = new List<DynamicFieldInputViewModel>
                {
                    new()
                    {
                        FieldDefinitionId = fieldId,
                        NomeCampo = "Lunghezza",
                        Etichetta = "Lunghezza",
                        TipoDato = DynamicFieldType.Decimal,
                        Template = DynamicFieldTemplate.Standard,
                        Valore = "105"
                    }
                }
            };

            await service.UpdateDynamicMeasurementAsync(model);
        }

        using (var assertContext = factory.CreateContext())
        {
            var values = await assertContext.DynamicMeasurementValues
                .Where(x => x.DynamicMeasurementRecordId == recordId)
                .ToListAsync();

            Assert.Single(values);
            Assert.Equal("105", values[0].Valore);
        }
    }
}