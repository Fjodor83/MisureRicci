using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MisureRicci.Services
{
    public class MeasurementService : IMeasurementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IReadOnlyDictionary<string, Func<int, Task<object?>>> _measurementLoaders;
        private readonly IReadOnlyDictionary<string, Func<object, Task<bool>>> _measurementUpdaters;

        public MeasurementService(ApplicationDbContext context)
        {
            _context = context;
            _measurementLoaders = new Dictionary<string, Func<int, Task<object?>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["giacca"] = id => GetMeasurementEntityAsync(_context.MisureGiacca, id, query => query.Include(m => m.Cliente)),
                ["pantalone"] = id => GetMeasurementEntityAsync(_context.MisurePantalone, id, query => query.Include(m => m.Cliente)),
                ["abito"] = id => GetMeasurementEntityAsync(_context.MisureAbitoCompleto, id, query => query.Include(m => m.Cliente).Include(m => m.Giacca).Include(m => m.Pantalone)),
                ["gilet"] = id => GetMeasurementEntityAsync(_context.MisureGilet, id, query => query.Include(m => m.Cliente)),
                ["maglie"] = id => GetMeasurementEntityAsync(_context.MisureMaglie, id, query => query.Include(m => m.Cliente)),
                ["outdoor"] = id => GetMeasurementEntityAsync(_context.MisureOutdoor, id, query => query.Include(m => m.Cliente)),
                ["camicia"] = id => GetMeasurementEntityAsync(_context.MisureCamicia, id, query => query.Include(m => m.Cliente)),
                ["scarpe"] = id => GetMeasurementEntityAsync(_context.MisureScarpe, id, query => query.Include(m => m.Cliente)),
                ["cravatta"] = id => GetMeasurementEntityAsync(_context.MisureCravatta, id, query => query.Include(m => m.Cliente)),
                ["cintura"] = id => GetMeasurementEntityAsync(_context.MisureCintura, id, query => query.Include(m => m.Cliente))
            };

            _measurementUpdaters = new Dictionary<string, Func<object, Task<bool>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["giacca"] = model => UpdateMeasurementEntityAsync<GiaccaMeasurement>(model),
                ["pantalone"] = model => UpdateMeasurementEntityAsync<PantaloneMeasurement>(model),
                ["abito"] = model => UpdateMeasurementEntityAsync<AbitoCompletoMeasurement>(model),
                ["gilet"] = model => UpdateMeasurementEntityAsync<GiletMeasurement>(model),
                ["maglie"] = model => UpdateMeasurementEntityAsync<MaglieMeasurement>(model),
                ["outdoor"] = model => UpdateMeasurementEntityAsync<OutdoorMeasurement>(model),
                ["camicia"] = model => UpdateMeasurementEntityAsync<CamiciaMeasurement>(model),
                ["scarpe"] = model => UpdateMeasurementEntityAsync<ScarpeMeasurement>(model),
                ["cravatta"] = model => UpdateMeasurementEntityAsync<CravattaMeasurement>(model),
                ["cintura"] = model => UpdateMeasurementEntityAsync<CinturaMeasurement>(model)
            };
        }

        public async Task<IEnumerable<MisureCliente>> GetGlobalRegistryAsync(string filter, int? negozioId, bool isAdmin)
        {
            var query = _context.RegistroMisure
                .AsNoTracking()
                .Include(m => m.Cliente)
                .AsQueryable();

            query = ApplyRegistryFilter(query, filter, negozioId, isAdmin);

            return await query.OrderByDescending(m => m.DataCreazione).ToListAsync();
        }

        public async Task<(IEnumerable<MisureCliente> Items, int TotalCount)> GetGlobalRegistryPagedAsync(string filter, int? negozioId, bool isAdmin, int page, int pageSize)
        {
            var query = _context.RegistroMisure
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
            var entry = await _context.RegistroMisure
                .Include(x => x.Cliente)
                .FirstOrDefaultAsync(x => x.Id == registryId);

            if (entry == null)
            {
                return null;
            }

            if (!isAdmin && negozioId.HasValue && entry.Cliente?.NegozioId != negozioId.Value)
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

                _context.RegistroMisure.Remove(entry);
                await _context.SaveChangesAsync();
                return clienteId;
            }

            var model = await GetMeasurementAsync(entry.RecordId, entry.TipoMisura);
            if (model != null)
            {
                _context.Remove(model);
            }

            _context.RegistroMisure.Remove(entry);
            await _context.SaveChangesAsync();
            return clienteId;
        }

        public async Task<object?> GetMeasurementScopedAsync(int id, string tipoMisura, int? negozioId, bool isAdmin)
        {
            var model = await GetMeasurementAsync(id, tipoMisura);
            if (model == null)
            {
                return null;
            }

            if (!isAdmin && negozioId.HasValue)
            {
                var baseMeasurement = model as BaseMeasurement;
                if (baseMeasurement?.Cliente?.NegozioId != negozioId.Value)
                {
                    return null;
                }
            }

            return model;
        }

        public async Task<bool> UpdateMeasurementAsync(object model, string tipoMisura)
        {
            if (_measurementUpdaters.TryGetValue(tipoMisura, out var updateHandler))
            {
                return await updateHandler(model);
            }

            return false;
        }

        public async Task<object?> GetMeasurementAsync(int id, string tipoMisura)
        {
            if (_measurementLoaders.TryGetValue(tipoMisura, out var loadHandler))
            {
                return await loadHandler(id);
            }

            return null;
        }

        public async Task DeleteMeasurementAsync(int id, string tipoMisura)
        {
            var model = await GetMeasurementAsync(id, tipoMisura);
            if (model != null)
            {
                _context.Remove(model);
                var registro = await _context.RegistroMisure.FirstOrDefaultAsync(r => r.RecordId == id && r.TipoMisura.ToLower() == tipoMisura.ToLower());
                if (registro != null)
                {
                    _context.RegistroMisure.Remove(registro);
                }
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddRegistryEntryAsync(int clienteId, string tipoMisura, string note, int recordId)
        {
            _context.RegistroMisure.Add(new MisureCliente { 
                ClienteId = clienteId, 
                TipoMisura = tipoMisura, 
                Note = note,
                RecordId = recordId
            });
            await _context.SaveChangesAsync();
        }

        private IQueryable<MisureCliente> ApplyRegistryFilter(IQueryable<MisureCliente> query, string filter, int? negozioId, bool isAdmin)
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

            return query;
        }

        private static async Task<object?> GetMeasurementEntityAsync<TEntity>(DbSet<TEntity> dbSet, int id, Func<IQueryable<TEntity>, IQueryable<TEntity>> include)
            where TEntity : class
        {
            return await include(dbSet.AsQueryable()).FirstOrDefaultAsync(BuildIdPredicate<TEntity>(id));
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

        private static System.Linq.Expressions.Expression<Func<TEntity, bool>> BuildIdPredicate<TEntity>(int id)
            where TEntity : class
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TEntity), "entity");
            var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseMeasurement.Id));
            var constant = System.Linq.Expressions.Expression.Constant(id);
            var body = System.Linq.Expressions.Expression.Equal(property, constant);
            return System.Linq.Expressions.Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        }

        public async Task CreateGiaccaAsync(GiaccaMeasurement model) { _context.MisureGiacca.Add(model); await _context.SaveChangesAsync(); await AddRegistryEntryAsync(model.ClienteId, "Giacca", "Nuova misura giacca registrata", model.Id); }
        public async Task CreatePantaloneAsync(PantaloneMeasurement model) { _context.MisurePantalone.Add(model); await _context.SaveChangesAsync(); await AddRegistryEntryAsync(model.ClienteId, "Pantalone", "Nuova misura pantalone registrata", model.Id); }
        public async Task CreateGiletAsync(GiletMeasurement model) { _context.MisureGilet.Add(model); await _context.SaveChangesAsync(); await AddRegistryEntryAsync(model.ClienteId, "Gilet", "Nuova misura gilet registrata", model.Id); }
        public async Task CreateMaglieAsync(MaglieMeasurement model) { _context.MisureMaglie.Add(model); await _context.SaveChangesAsync(); await AddRegistryEntryAsync(model.ClienteId, "Maglie", "Nuova misura maglieria registrata", model.Id); }
        public async Task CreateOutdoorAsync(OutdoorMeasurement model) { _context.MisureOutdoor.Add(model); await _context.SaveChangesAsync(); await AddRegistryEntryAsync(model.ClienteId, "Outdoor", "Nuova misura outdoor registrata", model.Id); }
        
        public async Task CreateAbitoAsync(AbitoCompletoMeasurement model) 
        { 
            if (model.Giacca != null) model.Giacca.ClienteId = model.ClienteId;
            if (model.Pantalone != null) model.Pantalone.ClienteId = model.ClienteId;
            _context.MisureAbitoCompleto.Add(model); 
            await _context.SaveChangesAsync(); 
            await AddRegistryEntryAsync(model.ClienteId, "Abito", "Nuova misura abito completo registrata", model.Id); 
        }

        public async Task CreateCamiciaAsync(CamiciaMeasurement model) { _context.MisureCamicia.Add(model); await _context.SaveChangesAsync(); await AddRegistryEntryAsync(model.ClienteId, "Camicia", "Nuova misura camicia registrata", model.Id); }
        public async Task CreateScarpeAsync(ScarpeMeasurement model) { _context.MisureScarpe.Add(model); await _context.SaveChangesAsync(); await AddRegistryEntryAsync(model.ClienteId, "Scarpe", "Nuova misura scarpe registrata", model.Id); }
        public async Task CreateCravattaAsync(CravattaMeasurement model) { _context.MisureCravatta.Add(model); await _context.SaveChangesAsync(); await AddRegistryEntryAsync(model.ClienteId, "Cravatta", "Nuova misura cravatta registrata", model.Id); }
        public async Task CreateCinturaAsync(CinturaMeasurement model) { _context.MisureCintura.Add(model); await _context.SaveChangesAsync(); await AddRegistryEntryAsync(model.ClienteId, "Cintura", "Nuova misura cintura registrata", model.Id); }

        public async Task UpdateGiaccaAsync(GiaccaMeasurement model) { _context.Update(model); await _context.SaveChangesAsync(); }
        public async Task UpdatePantaloneAsync(PantaloneMeasurement model) { _context.Update(model); await _context.SaveChangesAsync(); }
        public async Task UpdateGiletAsync(GiletMeasurement model) { _context.Update(model); await _context.SaveChangesAsync(); }
        public async Task UpdateMaglieAsync(MaglieMeasurement model) { _context.Update(model); await _context.SaveChangesAsync(); }
        public async Task UpdateOutdoorAsync(OutdoorMeasurement model) { _context.Update(model); await _context.SaveChangesAsync(); }
        public async Task UpdateAbitoAsync(AbitoCompletoMeasurement model) { _context.Update(model); await _context.SaveChangesAsync(); }
        public async Task UpdateCamiciaAsync(CamiciaMeasurement model) { _context.Update(model); await _context.SaveChangesAsync(); }
        public async Task UpdateScarpeAsync(ScarpeMeasurement model) { _context.Update(model); await _context.SaveChangesAsync(); }
        public async Task UpdateCravattaAsync(CravattaMeasurement model) { _context.Update(model); await _context.SaveChangesAsync(); }
        public async Task UpdateCinturaAsync(CinturaMeasurement model) { _context.Update(model); await _context.SaveChangesAsync(); }
    }
}
