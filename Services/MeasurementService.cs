using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public class MeasurementService : IMeasurementService, IMeasurementRegistryService, ILegacyMeasurementService
    {
        private readonly ApplicationDbContext _context;
        private static readonly IReadOnlyDictionary<TipoMisuraLegacy, Func<MeasurementService, int, Task<object?>>> MeasurementLoaders
            = new Dictionary<TipoMisuraLegacy, Func<MeasurementService, int, Task<object?>>>()
        {
            [TipoMisuraLegacy.Giacca] = async (service, id) => await service._context.MisureGiacca.AsNoTracking().Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
            [TipoMisuraLegacy.Pantalone] = async (service, id) => await service._context.MisurePantalone.AsNoTracking().Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
            [TipoMisuraLegacy.Abito] = async (service, id) => await service._context.MisureAbitoCompleto.AsNoTracking()
                             .Include(m => m.Cliente)
                             .Include(m => m.Giacca)
                             .Include(m => m.Pantalone)
                             .FirstOrDefaultAsync(m => m.Id == id),
            [TipoMisuraLegacy.Gilet] = async (service, id) => await service._context.MisureGilet.AsNoTracking().Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
            [TipoMisuraLegacy.Maglie] = async (service, id) => await service._context.MisureMaglie.AsNoTracking().Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
            [TipoMisuraLegacy.Outdoor] = async (service, id) => await service._context.MisureOutdoor.AsNoTracking().Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
            [TipoMisuraLegacy.Camicia] = async (service, id) => await service._context.MisureCamicia.AsNoTracking().Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
            [TipoMisuraLegacy.Scarpe] = async (service, id) => await service._context.MisureScarpe.AsNoTracking().Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
            [TipoMisuraLegacy.Cravatta] = async (service, id) => await service._context.MisureCravatta.AsNoTracking().Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
            [TipoMisuraLegacy.Cintura] = async (service, id) => await service._context.MisureCintura.AsNoTracking().Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id)
        };

        private static readonly IReadOnlyDictionary<TipoMisuraLegacy, Func<MeasurementService, object, Task<bool>>> MeasurementUpdaters
            = new Dictionary<TipoMisuraLegacy, Func<MeasurementService, object, Task<bool>>>()
        {
            [TipoMisuraLegacy.Giacca] = (service, model) => service.UpdateMeasurementEntityAsync<GiaccaMeasurement>(model),
            [TipoMisuraLegacy.Pantalone] = (service, model) => service.UpdateMeasurementEntityAsync<PantaloneMeasurement>(model),
            [TipoMisuraLegacy.Abito] = (service, model) => service.UpdateMeasurementEntityAsync<AbitoCompletoMeasurement>(model),
            [TipoMisuraLegacy.Gilet] = (service, model) => service.UpdateMeasurementEntityAsync<GiletMeasurement>(model),
            [TipoMisuraLegacy.Maglie] = (service, model) => service.UpdateMeasurementEntityAsync<MaglieMeasurement>(model),
            [TipoMisuraLegacy.Outdoor] = (service, model) => service.UpdateMeasurementEntityAsync<OutdoorMeasurement>(model),
            [TipoMisuraLegacy.Camicia] = (service, model) => service.UpdateMeasurementEntityAsync<CamiciaMeasurement>(model),
            [TipoMisuraLegacy.Scarpe] = (service, model) => service.UpdateMeasurementEntityAsync<ScarpeMeasurement>(model),
            [TipoMisuraLegacy.Cravatta] = (service, model) => service.UpdateMeasurementEntityAsync<CravattaMeasurement>(model),
            [TipoMisuraLegacy.Cintura] = (service, model) => service.UpdateMeasurementEntityAsync<CinturaMeasurement>(model)
        };

        public MeasurementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MisureCliente>> GetGlobalRegistryAsync(string? filter, int? negozioId, bool isAdmin)
        {
            if (!CanAccessTenant(negozioId, isAdmin))
            {
                return Array.Empty<MisureCliente>();
            }

            var query = _context.Misure
                .AsNoTracking()
                .Include(m => m.Cliente)
                .AsQueryable();

            query = ApplyRegistryFilter(query, filter, negozioId, isAdmin);

            return await query.OrderByDescending(m => m.DataCreazione).ToListAsync();
        }

        public async Task<(IEnumerable<MisureCliente> Items, int TotalCount)> GetGlobalRegistryPagedAsync(string? filter, int? negozioId, bool isAdmin, int page, int pageSize)
        {
            if (!CanAccessTenant(negozioId, isAdmin))
            {
                return (Array.Empty<MisureCliente>(), 0);
            }

            var query = _context.Misure
                .AsNoTracking()
                .Include(m => m.Cliente)
                .AsQueryable();

            query = ApplyRegistryFilter(query, filter, negozioId, isAdmin);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(m => m.DataCreazione)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<MisureCliente?> GetRegistryEntryAsync(int registryId, int? negozioId, bool isAdmin)
        {
            if (!CanAccessTenant(negozioId, isAdmin))
            {
                return null;
            }

            var query = _context.Misure
                .AsNoTracking()
                .Include(x => x.Cliente)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(x => x.Cliente != null && x.Cliente.NegozioId == negozioId!.Value);
            }

            var entry = await query.FirstOrDefaultAsync(x => x.Id == registryId);

            if (entry == null)
            {
                return null;
            }

            return entry;
        }

        public async Task<object?> GetMeasurementByRegistryEntryAsync(int registryId, int? negozioId, bool isAdmin)
        {
            var entry = await GetRegistryEntryAsync(registryId, negozioId, isAdmin);
            if (entry == null || entry.IsDynamic)
            {
                return null;
            }

            return await GetMeasurementAsync(entry.RecordId, entry.TipoMisura);
        }

        public async Task<int?> DeleteByRegistryEntryAsync(int registryId, int? negozioId, bool isAdmin)
        {
            var entry = await GetRegistryEntryAsync(registryId, negozioId, isAdmin);
            if (entry == null)
            {
                return null;
            }

            var clienteId = entry.ClienteId;

            if (entry.IsDynamic)
            {
                var dynamicRecord = await _context.DynamicMeasurementRecords
                    .FirstOrDefaultAsync(x => x.Id == entry.RecordId);
                if (dynamicRecord != null)
                {
                    _context.DynamicMeasurementRecords.Remove(dynamicRecord);
                }

                var trackedDynamicEntry = await _context.Misure
                    .FirstOrDefaultAsync(x => x.Id == entry.Id);
                if (trackedDynamicEntry != null)
                {
                    _context.Misure.Remove(trackedDynamicEntry);
                }

                await _context.SaveChangesAsync();
                return clienteId;
            }

            var model = await GetMeasurementAsync(entry.RecordId, entry.TipoMisura);
            if (model != null)
            {
                _context.Remove(model);
            }

            var trackedEntry = await _context.Misure
                .FirstOrDefaultAsync(x => x.Id == entry.Id);
            if (trackedEntry != null)
            {
                _context.Misure.Remove(trackedEntry);
            }

            await _context.SaveChangesAsync();
            return clienteId;
        }

        public async Task<object?> GetMeasurementScopedAsync(int id, string tipoMisura, int? negozioId, bool isAdmin)
        {
            if (!CanAccessTenant(negozioId, isAdmin))
            {
                return null;
            }

            var model = await GetMeasurementAsync(id, tipoMisura);
            if (model == null)
            {
                return null;
            }

            if (!isAdmin)
            {
                var baseMeasurement = model as BaseMeasurement;
                if (baseMeasurement?.Cliente?.NegozioId != negozioId)
                {
                    return null;
                }
            }

            return model;
        }

        public async Task<bool> UpdateMeasurementAsync(object model, string tipoMisura)
        {
            if (TryParseTipoMisura(tipoMisura, out var tipo) && MeasurementUpdaters.TryGetValue(tipo, out var updateHandler))
            {
                return await updateHandler(this, model);
            }

            return false;
        }

        public async Task<object?> GetMeasurementAsync(int id, string tipoMisura)
        {
            if (TryParseTipoMisura(tipoMisura, out var tipo) && MeasurementLoaders.TryGetValue(tipo, out var loadHandler))
            {
                return await loadHandler(this, id);
            }

            return null;
        }

        public async Task<bool> DeleteMeasurementAsync(int id, string tipoMisura, int? negozioId, bool isAdmin)
        {
            var model = await GetMeasurementScopedAsync(id, tipoMisura, negozioId, isAdmin);
            if (model == null)
            {
                return false;
            }

            _context.Remove(model);
            var registro = await _context.Misure
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.RecordId == id && r.TipoMisura.ToLower() == tipoMisura.ToLower());
            if (registro != null)
            {
                if (isAdmin || (negozioId.HasValue && registro.Cliente != null && registro.Cliente.NegozioId == negozioId.Value))
                {
                    _context.Misure.Remove(registro);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<MisureCliente> ApplyRegistryFilter(IQueryable<MisureCliente> query, string? filter, int? negozioId, bool isAdmin)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var normalizedFilter = filter.ToLowerInvariant();
                query = normalizedFilter switch
                {
                    "giacca" => query.Where(m => m.TipoMisura.Contains("Giacca") || m.TipoMisura.Contains("Abito") || m.TipoMisura.Contains("Outdoor")),
                    "pantalone" => query.Where(m => m.TipoMisura.Contains("Pantalone") || m.TipoMisura.Contains("Abito")),
                    "camicia" => query.Where(m => m.TipoMisura.Contains("Camicia") || m.TipoMisura.Contains("Maglie")),
                    "scarpe" => query.Where(m => m.TipoMisura.Contains("Scarpe") || m.TipoMisura.Contains("Cintura") || m.TipoMisura.Contains("Cravatta")),
                    _ => query.Where(m => m.TipoMisura.ToLower().Contains(normalizedFilter))
                };
            }

            if (!isAdmin && negozioId.HasValue)
            {
                query = query.Where(m => m.Cliente != null && m.Cliente.NegozioId == negozioId.Value);
            }
            else if (!isAdmin)
            {
                query = query.Where(_ => false);
            }

            return query;
        }

        private async Task<bool> UpdateMeasurementEntityAsync<TEntity>(object model)
            where TEntity : class
        {
            if (model is not TEntity entity)
            {
                return false;
            }

            _context.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static bool TryParseTipoMisura(string tipoMisura, out TipoMisuraLegacy tipo)
        {
            return Enum.TryParse(tipoMisura, ignoreCase: true, out tipo);
        }

        private static bool CanAccessTenant(int? negozioId, bool isAdmin)
        {
            return isAdmin || negozioId.HasValue;
        }
    }
}
