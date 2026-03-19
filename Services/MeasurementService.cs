using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MisureRicci.Services
{
    public class MeasurementService : IMeasurementService
    {
        private readonly ApplicationDbContext _context;

        public MeasurementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MisureCliente>> GetGlobalRegistryAsync(string filter, int? negozioId, bool isAdmin)
        {
            var query = _context.RegistroMisure
                .Include(m => m.Cliente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
                if (filter == "giacca") query = query.Where(m => m.TipoMisura.Contains("Giacca") || m.TipoMisura.Contains("Abito") || m.TipoMisura.Contains("Outdoor"));
                else if (filter == "pantalone") query = query.Where(m => m.TipoMisura.Contains("Pantalone") || m.TipoMisura.Contains("Abito"));
                else if (filter == "camicia") query = query.Where(m => m.TipoMisura.Contains("Camicia") || m.TipoMisura.Contains("Maglie"));
                else if (filter == "scarpe") query = query.Where(m => m.TipoMisura.Contains("Scarpe") || m.TipoMisura.Contains("Cintura") || m.TipoMisura.Contains("Cravatta"));
                else query = query.Where(m => m.TipoMisura.ToLower().Contains(filter));
            }

            if (!isAdmin && negozioId.HasValue)
            {
                query = query.Where(m => m.Cliente != null && m.Cliente.NegozioId == negozioId.Value);
            }

            return await query.OrderByDescending(m => m.DataCreazione).ToListAsync();
        }

        public async Task<(IEnumerable<MisureCliente> Items, int TotalCount)> GetGlobalRegistryPagedAsync(string filter, int? negozioId, bool isAdmin, int page, int pageSize)
        {
            var query = _context.RegistroMisure
                .Include(m => m.Cliente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
                if (filter == "giacca") query = query.Where(m => m.TipoMisura.Contains("Giacca") || m.TipoMisura.Contains("Abito") || m.TipoMisura.Contains("Outdoor"));
                else if (filter == "pantalone") query = query.Where(m => m.TipoMisura.Contains("Pantalone") || m.TipoMisura.Contains("Abito"));
                else if (filter == "camicia") query = query.Where(m => m.TipoMisura.Contains("Camicia") || m.TipoMisura.Contains("Maglie"));
                else if (filter == "scarpe") query = query.Where(m => m.TipoMisura.Contains("Scarpe") || m.TipoMisura.Contains("Cintura") || m.TipoMisura.Contains("Cravatta"));
                else query = query.Where(m => m.TipoMisura.ToLower().Contains(filter));
            }

            if (!isAdmin && negozioId.HasValue)
            {
                query = query.Where(m => m.Cliente != null && m.Cliente.NegozioId == negozioId.Value);
            }

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
            switch (tipoMisura.ToLowerInvariant())
            {
                case "giacca" when model is GiaccaMeasurement giacca:
                    await UpdateGiaccaAsync(giacca);
                    return true;
                case "pantalone" when model is PantaloneMeasurement pantalone:
                    await UpdatePantaloneAsync(pantalone);
                    return true;
                case "abito" when model is AbitoCompletoMeasurement abito:
                    await UpdateAbitoAsync(abito);
                    return true;
                case "gilet" when model is GiletMeasurement gilet:
                    await UpdateGiletAsync(gilet);
                    return true;
                case "maglie" when model is MaglieMeasurement maglie:
                    await UpdateMaglieAsync(maglie);
                    return true;
                case "outdoor" when model is OutdoorMeasurement outdoor:
                    await UpdateOutdoorAsync(outdoor);
                    return true;
                case "camicia" when model is CamiciaMeasurement camicia:
                    await UpdateCamiciaAsync(camicia);
                    return true;
                case "scarpe" when model is ScarpeMeasurement scarpe:
                    await UpdateScarpeAsync(scarpe);
                    return true;
                case "cravatta" when model is CravattaMeasurement cravatta:
                    await UpdateCravattaAsync(cravatta);
                    return true;
                case "cintura" when model is CinturaMeasurement cintura:
                    await UpdateCinturaAsync(cintura);
                    return true;
                default:
                    return false;
            }
        }

        public async Task<object?> GetMeasurementAsync(int id, string tipoMisura)
        {
            return tipoMisura.ToLower() switch
            {
                "giacca" => await _context.MisureGiacca.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "pantalone" => await _context.MisurePantalone.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "abito" => await _context.MisureAbitoCompleto.Include(m => m.Cliente).Include(m => m.Giacca).Include(m => m.Pantalone).FirstOrDefaultAsync(m => m.Id == id),
                "gilet" => await _context.MisureGilet.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "maglie" => await _context.MisureMaglie.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "outdoor" => await _context.MisureOutdoor.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "camicia" => await _context.MisureCamicia.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "scarpe" => await _context.MisureScarpe.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "cravatta" => await _context.MisureCravatta.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                "cintura" => await _context.MisureCintura.Include(m => m.Cliente).FirstOrDefaultAsync(m => m.Id == id),
                _ => null
            };
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
