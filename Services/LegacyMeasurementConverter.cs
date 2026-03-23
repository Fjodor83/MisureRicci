using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    /// <inheritdoc cref="ILegacyMeasurementConverter"/>
    public class LegacyMeasurementConverter : ILegacyMeasurementConverter
    {
        private readonly ApplicationDbContext _context;

        public LegacyMeasurementConverter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DynamicMeasurementRecord?> ConvertAsync(
            BaseMeasurement legacy,
            string tipoMisura,
            string createdByUserId,
            CancellationToken cancellationToken = default)
        {
            // Locate the matching MeasurementType by name (case-insensitive)
            var measurementType = await _context.MeasurementTypes
                .AsNoTracking()
                .Include(t => t.Campi)
                .FirstOrDefaultAsync(
                    t => t.Nome.ToLower() == tipoMisura.ToLower() && t.IsActive,
                    cancellationToken);

            if (measurementType == null)
                return null;

            // Extract all public double properties from the concrete entity type
            // (Id, ClienteId etc. are int/string so they won't match)
            var legacyValues = legacy.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(double))
                .ToDictionary(
                    p => p.Name,
                    p => ((double)p.GetValue(legacy)!).ToString(CultureInfo.InvariantCulture),
                    StringComparer.OrdinalIgnoreCase);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var record = new DynamicMeasurementRecord
            {
                ClienteId = legacy.ClienteId,
                MeasurementTypeId = measurementType.Id,
                CreatedByUserId = createdByUserId,
                CreatedAt = legacy.CreatedAt
            };

            _context.DynamicMeasurementRecords.Add(record);
            await _context.SaveChangesAsync(cancellationToken);

            // Map each active field definition whose NomeCampo matches a legacy property
            var values = measurementType.Campi
                .Where(f => f.IsActive && legacyValues.ContainsKey(f.NomeCampo))
                .Select(f => new DynamicMeasurementValue
                {
                    DynamicMeasurementRecordId = record.Id,
                    MeasurementFieldDefinitionId = f.Id,
                    Valore = legacyValues[f.NomeCampo]
                })
                .ToList();

            if (values.Count > 0)
                _context.DynamicMeasurementValues.AddRange(values);

            // Create a new registry entry pointing to the dynamic record
            _context.RegistroMisure.Add(new MisureCliente
            {
                ClienteId = legacy.ClienteId,
                TipoMisura = measurementType.Nome,
                Note = $"Convertito da misura legacy {tipoMisura} (ID legacy: {legacy.Id})",
                RecordId = record.Id,
                IsDynamic = true
            });

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return record;
        }
    }
}
