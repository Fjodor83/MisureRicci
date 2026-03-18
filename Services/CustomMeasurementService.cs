using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public class CustomMeasurementService : ICustomMeasurementService
    {
        private readonly ApplicationDbContext _context;

        public CustomMeasurementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MeasurementType>> GetMeasurementTypesAsync(bool onlyActive = true)
        {
            var query = _context.MeasurementTypes.AsQueryable();
            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            return await query
                .OrderBy(x => x.Nome)
                .ToListAsync();
        }

        public async Task<MeasurementType?> GetMeasurementTypeByIdAsync(int id)
        {
            return await _context.MeasurementTypes
                .Include(x => x.Campi.OrderBy(f => f.OrdineGruppo).ThenBy(f => f.Gruppo).ThenBy(f => f.Ordine).ThenBy(f => f.Etichetta))
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<MeasurementType> CreateMeasurementTypeAsync(MeasurementType model)
        {
            model.Nome = model.Nome.Trim();
            _context.MeasurementTypes.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task UpdateMeasurementTypeAsync(MeasurementType model)
        {
            model.Nome = model.Nome.Trim();
            _context.MeasurementTypes.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMeasurementTypeAsync(int id)
        {
            var entity = await _context.MeasurementTypes
                .Include(x => x.Campi)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null || entity.IsSystem)
            {
                return;
            }

            _context.MeasurementTypes.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MeasurementFieldDefinition>> GetFieldsByTypeAsync(int measurementTypeId, bool onlyActive = true)
        {
            var query = _context.MeasurementFieldDefinitions
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
        }

        public async Task<MeasurementFieldDefinition?> GetFieldByIdAsync(int id)
        {
            return await _context.MeasurementFieldDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<MeasurementFieldDefinition> CreateFieldAsync(MeasurementFieldDefinition model)
        {
            model.NomeCampo = model.NomeCampo.Trim();
            model.Etichetta = model.Etichetta.Trim();
            model.Gruppo = string.IsNullOrWhiteSpace(model.Gruppo) ? null : model.Gruppo.Trim();
            model.HelpText = string.IsNullOrWhiteSpace(model.HelpText) ? null : model.HelpText.Trim();
            _context.MeasurementFieldDefinitions.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task UpdateFieldAsync(MeasurementFieldDefinition model)
        {
            model.NomeCampo = model.NomeCampo.Trim();
            model.Etichetta = model.Etichetta.Trim();
            model.Gruppo = string.IsNullOrWhiteSpace(model.Gruppo) ? null : model.Gruppo.Trim();
            model.HelpText = string.IsNullOrWhiteSpace(model.HelpText) ? null : model.HelpText.Trim();
            _context.MeasurementFieldDefinitions.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFieldAsync(int id)
        {
            var entity = await _context.MeasurementFieldDefinitions.FindAsync(id);
            if (entity == null)
            {
                return;
            }

            _context.MeasurementFieldDefinitions.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<DynamicMeasurementRecord> CreateDynamicMeasurementAsync(DynamicMeasurementCreateViewModel model, string? createdByUserId)
        {
            var fields = await GetFieldsByTypeAsync(model.MeasurementTypeId, onlyActive: true);

            foreach (var field in fields.Where(x => x.Obbligatorio && !IsStructuralTemplate(x.Template)))
            {
                var value = model.Fields.FirstOrDefault(x => x.FieldDefinitionId == field.Id)?.Valore;
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException($"Il campo '{field.Etichetta}' è obbligatorio.");
                }
            }

            var record = new DynamicMeasurementRecord
            {
                ClienteId = model.ClienteId,
                MeasurementTypeId = model.MeasurementTypeId,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.DynamicMeasurementRecords.Add(record);
            await _context.SaveChangesAsync();

            var values = model.Fields
                .Where(x => !string.IsNullOrWhiteSpace(x.Valore) && !IsStructuralTemplate(x.Template))
                .Select(x => new DynamicMeasurementValue
                {
                    DynamicMeasurementRecordId = record.Id,
                    MeasurementFieldDefinitionId = x.FieldDefinitionId,
                    Valore = x.Valore!.Trim()
                })
                .ToList();

            if (values.Count > 0)
            {
                _context.DynamicMeasurementValues.AddRange(values);
            }

            var type = await _context.MeasurementTypes.FirstAsync(x => x.Id == model.MeasurementTypeId);
            _context.RegistroMisure.Add(new MisureCliente
            {
                ClienteId = model.ClienteId,
                TipoMisura = type.Nome,
                Note = "Misura dinamica registrata",
                RecordId = record.Id,
                IsDynamic = true,
                DataCreazione = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return record;
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
                ClienteNome = $"{record.Cliente?.Nome} {record.Cliente?.Cognome}".Trim(),
                TipoNome = record.MeasurementType?.Nome ?? string.Empty,
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
                    Valore = valuesByFieldId.TryGetValue(f.Id, out var value) ? value : null
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

            var fields = await GetFieldsByTypeAsync(record.MeasurementTypeId, onlyActive: true);
            foreach (var field in fields.Where(x => x.Obbligatorio && !IsStructuralTemplate(x.Template)))
            {
                var value = model.Fields.FirstOrDefault(x => x.FieldDefinitionId == field.Id)?.Valore;
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException($"Il campo '{field.Etichetta}' è obbligatorio.");
                }
            }

            _context.DynamicMeasurementValues.RemoveRange(record.Values);

            var values = model.Fields
                .Where(x => !string.IsNullOrWhiteSpace(x.Valore) && !IsStructuralTemplate(x.Template))
                .Select(x => new DynamicMeasurementValue
                {
                    DynamicMeasurementRecordId = record.Id,
                    MeasurementFieldDefinitionId = x.FieldDefinitionId,
                    Valore = x.Valore!.Trim()
                });

            _context.DynamicMeasurementValues.AddRange(values);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDynamicMeasurementAsync(int recordId)
        {
            var record = await _context.DynamicMeasurementRecords.FirstOrDefaultAsync(x => x.Id == recordId);
            if (record == null)
            {
                return;
            }

            var registro = await _context.RegistroMisure.FirstOrDefaultAsync(x => x.RecordId == recordId && x.IsDynamic);
            if (registro != null)
            {
                _context.RegistroMisure.Remove(registro);
            }

            _context.DynamicMeasurementRecords.Remove(record);
            await _context.SaveChangesAsync();
        }

        private static bool IsStructuralTemplate(DynamicFieldTemplate template)
        {
            return template == DynamicFieldTemplate.SectionHeader
                || template == DynamicFieldTemplate.Divider
                || template == DynamicFieldTemplate.AlertNote;
        }
    }
}
