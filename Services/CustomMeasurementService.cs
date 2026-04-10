using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;
using System.Security.Claims;

namespace MisureRicci.Services
{
    public class CustomMeasurementService : ICustomMeasurementService
    {


        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IAuditService _auditService;

        public CustomMeasurementService(ApplicationDbContext context, IMemoryCache cache, IAuditService auditService)
        {
            _context = context;
            _cache = cache;
            _auditService = auditService;
        }

        public async Task<List<MeasurementType>> GetMeasurementTypesAsync(bool onlyActive = true)
        {
            var cacheKey = onlyActive ? CacheKeys.MeasurementTypesActive : CacheKeys.MeasurementTypesAll;
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var query = _context.DynamicMeasurementTypes.AsNoTracking().AsQueryable();
                if (onlyActive)
                {
                    query = query.Where(x => x.IsActive);
                }

                return await query
                    .OrderBy(x => x.Nome)
                    .ToListAsync();
            }) ?? new List<MeasurementType>();
        }

        public async Task<MeasurementType?> GetMeasurementTypeByIdAsync(int id)
        {
            return await _context.DynamicMeasurementTypes
                .Include(x => x.Campi.OrderBy(f => f.OrdineGruppo).ThenBy(f => f.Gruppo).ThenBy(f => f.Ordine).ThenBy(f => f.Etichetta))
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<MeasurementType> CreateMeasurementTypeAsync(MeasurementType model)
        {
            model.Nome = model.Nome.Trim();
            _context.DynamicMeasurementTypes.Add(model);
            await _context.SaveChangesAsync();
            InvalidateMeasurementTypeCaches();
            return model;
        }

        public async Task UpdateMeasurementTypeAsync(MeasurementType model)
        {
            model.Nome = model.Nome.Trim();
            _context.DynamicMeasurementTypes.Update(model);
            await _context.SaveChangesAsync();
            InvalidateMeasurementTypeCaches();
            InvalidateFieldCaches(model.Id);
        }

        public async Task DeleteMeasurementTypeAsync(int id)
        {
            var entity = await _context.DynamicMeasurementTypes
                .Include(x => x.Campi)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null || entity.IsSystem)
            {
                return;
            }

            _context.DynamicMeasurementTypes.Remove(entity);
            await _context.SaveChangesAsync();
            InvalidateMeasurementTypeCaches();
            InvalidateFieldCaches(id);
        }

        public async Task<List<MeasurementFieldDefinition>> GetFieldsByTypeAsync(int measurementTypeId, bool onlyActive = true)
        {
            var cacheKey = BuildFieldCacheKey(measurementTypeId, onlyActive);
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var query = _context.DynamicFieldDefinitions
                    .AsNoTracking()
                    .Where(x => x.MeasurementTypeId == measurementTypeId);

                if (onlyActive)
                {
                    query = query.Where(x => x.IsActive);
                }

                return await query
                    .OrderBy(x => x.OrdineGruppo)
                    .ThenBy(x => x.Gruppo)
                    .ThenBy(x => x.Ordine)
                    .ThenBy(x => x.Etichetta)
                    .ToListAsync();
            }) ?? new List<MeasurementFieldDefinition>();
        }

        public async Task<MeasurementFieldDefinition?> GetFieldByIdAsync(int id)
        {
            return await _context.DynamicFieldDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<MeasurementFieldDefinition> CreateFieldAsync(MeasurementFieldDefinition model)
        {
            model.NomeCampo = model.NomeCampo.Trim();
            model.Etichetta = model.Etichetta.Trim();
            model.Gruppo = string.IsNullOrWhiteSpace(model.Gruppo) ? null : model.Gruppo.Trim();
            model.HelpText = string.IsNullOrWhiteSpace(model.HelpText) ? null : model.HelpText.Trim();
            _context.DynamicFieldDefinitions.Add(model);
            await _context.SaveChangesAsync();
            InvalidateFieldCaches(model.MeasurementTypeId);
            return model;
        }

        public async Task UpdateFieldAsync(MeasurementFieldDefinition model)
        {
            model.NomeCampo = model.NomeCampo.Trim();
            model.Etichetta = model.Etichetta.Trim();
            model.Gruppo = string.IsNullOrWhiteSpace(model.Gruppo) ? null : model.Gruppo.Trim();
            model.HelpText = string.IsNullOrWhiteSpace(model.HelpText) ? null : model.HelpText.Trim();
            _context.DynamicFieldDefinitions.Update(model);
            await _context.SaveChangesAsync();
            InvalidateFieldCaches(model.MeasurementTypeId);
        }

        public async Task DeleteFieldAsync(int id)
        {
            var entity = await _context.DynamicFieldDefinitions.FindAsync(id);
            if (entity == null)
            {
                return;
            }

            var measurementTypeId = entity.MeasurementTypeId;
            _context.DynamicFieldDefinitions.Remove(entity);
            await _context.SaveChangesAsync();
            InvalidateFieldCaches(measurementTypeId);
        }

        public async Task<DynamicMeasurementRecord> CreateDynamicMeasurementAsync(DynamicMeasurementCreateViewModel model, string? createdByUserId)
        {
            var fields = await GetFieldsByTypeAsync(model.MeasurementTypeId, onlyActive: true);
            var allowedFields = fields.ToDictionary(x => x.Id);
            EnsureFieldPayloadIsValid(model.Fields, allowedFields);
            ValidateRequiredFields(model.Fields, fields);

            var record = new DynamicMeasurementRecord
            {
                ClienteId = model.ClienteId,
                MeasurementTypeId = model.MeasurementTypeId,
                MeasurementUnit = model.SelectedUnit,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            var ownsTransaction = _context.Database.CurrentTransaction == null;
            await using var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;

            try
            {
                _context.DynamicMeasurementRecords.Add(record);
                await _context.SaveChangesAsync();

                var values = BuildDynamicMeasurementValues(model.Fields, allowedFields, record.Id, model.SelectedUnit);

                if (values.Count > 0)
                {
                    _context.DynamicMeasurementValues.AddRange(values);
                }

                var type = await _context.DynamicMeasurementTypes.FirstAsync(x => x.Id == model.MeasurementTypeId);
                _context.Misure.Add(new MisureCliente
                {
                    ClienteId = model.ClienteId,
                    TipoMisura = type.Nome,
                    SystemNote = "Misura dinamica registrata",
                    RecordId = record.Id,
                    IsDynamic = true,
                    DataCreazione = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                if (ownsTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                }

                await _auditService.WriteAsync("Misura", record.Id.ToString(), "Create", createdByUserId, null, type.Nome);

                return record;
            }
            catch
            {
                if (ownsTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                }

                throw;
            }
        }

        public async Task<DynamicMeasurementRecord?> GetDynamicMeasurementRecordByIdAsync(int id)
        {
            return await _context.DynamicMeasurementRecords
                .Include(x => x.Cliente)
                .Include(x => x.MeasurementType)
                .Include(x => x.Values)
                    .ThenInclude(v => v.MeasurementFieldDefinition)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<DynamicMeasurementCreateViewModel?> BuildDynamicMeasurementEditViewModelAsync(int recordId)
        {
            var record = await GetDynamicMeasurementRecordByIdAsync(recordId);
            if (record == null)
            {
                return null;
            }

            var fields = await GetFieldsByTypeAsync(record.MeasurementTypeId, onlyActive: true);
            var valuesByFieldId = record.Values.ToDictionary(x => x.MeasurementFieldDefinitionId, x => x.Valore);

            return new DynamicMeasurementCreateViewModel
            {
                RecordId = record.Id,
                ClienteId = record.ClienteId,
                MeasurementTypeId = record.MeasurementTypeId,
                SelectedUnit = record.MeasurementUnit,
                ClienteNome = $"{record.Cliente?.Nome} {record.Cliente?.Cognome}".Trim(),
                TipoNome = record.MeasurementType?.Nome ?? string.Empty,
                TypeImageUrl = record.MeasurementType?.ImageUrl,
                Fields = fields.Select(f => new DynamicFieldInputViewModel
                {
                    FieldDefinitionId = f.Id,
                    NomeCampo = f.NomeCampo,
                    Etichetta = f.Etichetta,
                    Gruppo = f.Gruppo,
                    OrdineGruppo = f.OrdineGruppo,
                    TipoDato = f.TipoDato,
                    Template = f.Template,
                    UnitaMisura = f.UnitaMisura,
                    Placeholder = f.Placeholder,
                    HelpText = f.HelpText,
                    Obbligatorio = f.Obbligatorio,
                    Ordine = f.Ordine,
                    Valore = valuesByFieldId.TryGetValue(f.Id, out var value)
                        ? MeasurementUnitHelper.ConvertStorageToDisplay(value, f.TipoDato, f.UnitaMisura, record.MeasurementUnit)
                        : null
                }).ToList()
            };
        }

        public async Task UpdateDynamicMeasurementAsync(DynamicMeasurementCreateViewModel model)
        {
            var record = await _context.DynamicMeasurementRecords
                .Include(x => x.Values)
                .FirstOrDefaultAsync(x => x.Id == model.RecordId);

            if (record == null)
            {
                throw new InvalidOperationException("Misura dinamica non trovata.");
            }

            if (record.ClienteId != model.ClienteId)
            {
                throw new InvalidOperationException("La misura dinamica non appartiene al cliente selezionato.");
            }

            if (record.MeasurementTypeId != model.MeasurementTypeId)
            {
                throw new InvalidOperationException("La misura dinamica non corrisponde alla tipologia selezionata.");
            }

            var fields = await GetFieldsByTypeAsync(record.MeasurementTypeId, onlyActive: true);
            var allowedFields = fields.ToDictionary(x => x.Id);
            EnsureFieldPayloadIsValid(model.Fields, allowedFields);
            ValidateRequiredFields(model.Fields, fields);

            var ownsTransaction = _context.Database.CurrentTransaction == null;
            await using var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;

            try
            {
                record.MeasurementUnit = model.SelectedUnit;
                _context.DynamicMeasurementValues.RemoveRange(record.Values);

                var values = BuildDynamicMeasurementValues(model.Fields, allowedFields, record.Id, model.SelectedUnit);

                _context.DynamicMeasurementValues.AddRange(values);
                await _context.SaveChangesAsync();

                if (ownsTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                }
            }
            catch
            {
                if (ownsTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                }

                throw;
            }
        }

        public async Task DeleteDynamicMeasurementAsync(int recordId)
        {
            var record = await _context.DynamicMeasurementRecords.FirstOrDefaultAsync(x => x.Id == recordId);
            if (record == null)
            {
                return;
            }

            var registro = await _context.Misure.FirstOrDefaultAsync(x => x.RecordId == recordId && x.IsDynamic);
            if (registro != null)
            {
                var isLinkedToCommessa = await _context.CommissioniMisureLinks
                    .AnyAsync(x => x.MisuraClienteId == registro.Id);
                if (isLinkedToCommessa)
                {
                    throw new InvalidOperationException("La misura e' collegata a una o piu' commesse. Scollegala prima di eliminarla.");
                }
            }

            var ownsTransaction = _context.Database.CurrentTransaction == null;
            await using var transaction = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;

            try
            {
                if (registro != null)
                {
                    _context.Misure.Remove(registro);
                }

                _context.DynamicMeasurementRecords.Remove(record);
                await _context.SaveChangesAsync();

                if (ownsTransaction && transaction != null)
                {
                    await transaction.CommitAsync();
                }
            }
            catch
            {
                if (ownsTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                }

                throw;
            }
        }

        private static bool IsStructuralTemplate(DynamicFieldTemplate template)
        {
            return template == DynamicFieldTemplate.SectionHeader
                || template == DynamicFieldTemplate.Divider
                || template == DynamicFieldTemplate.AlertNote;
        }

        private static void EnsureFieldPayloadIsValid(
            IEnumerable<DynamicFieldInputViewModel> submittedFields,
            IReadOnlyDictionary<int, MeasurementFieldDefinition> allowedFields)
        {
            var invalidFieldIds = submittedFields
                .Where(x => !allowedFields.ContainsKey(x.FieldDefinitionId))
                .Select(x => x.FieldDefinitionId)
                .Distinct()
                .ToList();

            if (invalidFieldIds.Count > 0)
            {
                throw new InvalidOperationException("Il form contiene campi non validi per la tipologia selezionata.");
            }
        }

        private static void ValidateRequiredFields(
            IEnumerable<DynamicFieldInputViewModel> submittedFields,
            IEnumerable<MeasurementFieldDefinition> fields)
        {
            foreach (var field in fields.Where(x => x.Obbligatorio && !IsStructuralTemplate(x.Template)))
            {
                var value = submittedFields.FirstOrDefault(x => x.FieldDefinitionId == field.Id)?.Valore;
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException($"Il campo '{field.Etichetta}' è obbligatorio.");
                }
            }
        }

        private static List<DynamicMeasurementValue> BuildDynamicMeasurementValues(
            IEnumerable<DynamicFieldInputViewModel> submittedFields,
            IReadOnlyDictionary<int, MeasurementFieldDefinition> allowedFields,
            int recordId,
            MeasurementUnit selectedUnit)
        {
            return submittedFields
                .Where(x =>
                    allowedFields.TryGetValue(x.FieldDefinitionId, out var definition)
                    && !string.IsNullOrWhiteSpace(x.Valore)
                    && !IsStructuralTemplate(definition.Template))
                .GroupBy(x => x.FieldDefinitionId)
                .Select(group =>
                {
                    var value = group.Last();
                    var definition = allowedFields[value.FieldDefinitionId];
                    return new DynamicMeasurementValue
                    {
                        DynamicMeasurementRecordId = recordId,
                        MeasurementFieldDefinitionId = value.FieldDefinitionId,
                        Valore = MeasurementUnitHelper.ConvertDisplayToStorage(
                            value.Valore,
                            definition.TipoDato,
                            definition.UnitaMisura,
                            selectedUnit)
                    };
                })
                .ToList();
        }

        public async Task<int?> GetRegistroMisuraIdByDynamicRecordAsync(int dynamicRecordId)
        {
            return await _context.Misure
                .Where(m => m.RecordId == dynamicRecordId && m.IsDynamic)
                .Select(m => (int?)m.Id)
                .FirstOrDefaultAsync();
        }

        private static string BuildFieldCacheKey(int measurementTypeId, bool onlyActive)
        {
            return CacheKeys.FieldDefinitions(measurementTypeId, onlyActive);
        }

        private void InvalidateMeasurementTypeCaches()
        {
            _cache.Remove(CacheKeys.MeasurementTypesActive);
            _cache.Remove(CacheKeys.MeasurementTypesAll);
        }

        private void InvalidateFieldCaches(int measurementTypeId)
        {
            _cache.Remove(BuildFieldCacheKey(measurementTypeId, onlyActive: true));
            _cache.Remove(BuildFieldCacheKey(measurementTypeId, onlyActive: false));
        }
    }
}
