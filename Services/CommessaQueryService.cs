using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public class CommessaQueryService : ICommessaQueryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public CommessaQueryService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<PagedResult<CommessaSartoriale>> GetCommissioniPagedAsync(
            int? clienteId, int? negozioId, bool isAdmin, int page, int pageSize)
        {
            var query = _context.Commissioni
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(c => c.ClienteId == clienteId.Value);

            if (!isAdmin)
            {
                if (!negozioId.HasValue)
                    return new PagedResult<CommessaSartoriale>(Array.Empty<CommessaSartoriale>(), 0, page, pageSize);
                query = query.Where(c => c.NegozioId == negozioId.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.DataApertura)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CommessaSartoriale>(items, totalCount, page, pageSize);
        }

        public async Task<CommessaKpiViewModel> GetKpiAsync(int? negozioId, bool isAdmin)
        {
            var query = _context.Commissioni.AsNoTracking().AsQueryable();

            if (!isAdmin)
            {
                if (!negozioId.HasValue) return new CommessaKpiViewModel();
                query = query.Where(c => c.NegozioId == negozioId.Value);
            }

            var now = DateTime.UtcNow;
            var snapshot = await query
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    Total = group.Count(),
                    Consegnate = group.Count(c => c.Stato == StatoCommessa.Consegnata),
                    Annullate = group.Count(c => c.Stato == StatoCommessa.Annullata),
                    InRitardo = group.Count(c =>
                        c.Stato != StatoCommessa.Consegnata &&
                        c.Stato != StatoCommessa.Annullata &&
                        c.DataConsegnaPrevista.HasValue &&
                        c.DataConsegnaPrevista.Value < now)
                })
                .FirstOrDefaultAsync();

            if (snapshot == null) return new CommessaKpiViewModel();

            return new CommessaKpiViewModel
            {
                Totale = snapshot.Total,
                Consegnate = snapshot.Consegnate,
                InCorso = snapshot.Total - snapshot.Consegnate - snapshot.Annullate,
                InRitardo = snapshot.InRitardo
            };
        }

        public async Task<CommessaSartoriale?> GetCommessaByIdAsync(int id, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.Commissioni
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .Include(c => c.Eventi.OrderByDescending(e => e.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (commessa == null) return null;
            if (!CommessaAccessHelper.CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin)) return null;
            return commessa;
        }

        public async Task<CommessaDetailsViewModel?> GetCommessaDetailsAsync(int id, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.Commissioni
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .Include(c => c.Eventi.OrderByDescending(e => e.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (commessa == null) return null;
            if (!CommessaAccessHelper.CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin)) return null;

            var misureConStatoLink = await _context.Misure
                .AsNoTracking()
                .Where(m => m.ClienteId == commessa.ClienteId)
                .GroupJoin(
                    _context.CommissioniMisureLinks
                        .AsNoTracking()
                        .Where(l => l.CommessaSartorialeId == commessa.Id),
                    misura => misura.Id,
                    link => link.MisuraClienteId,
                    (misura, links) => new
                    {
                        Item = new CommessaMisuraItem
                        {
                            MisuraClienteId = misura.Id,
                            RecordId = misura.RecordId,
                            TipoMisura = misura.TipoMisura,
                            IsDynamic = misura.IsDynamic,
                            DataCreazione = misura.DataCreazione,
                            Note = misura.Note ?? misura.SystemNote
                        },
                        IsLinked = links.Any()
                    })
                .OrderByDescending(x => x.Item.DataCreazione)
                .ToListAsync();

            var linked = misureConStatoLink
                .Where(x => x.IsLinked)
                .Select(x =>
                {
                    x.Item.IsRecommended = CommessaAccessHelper.IsMeasurementRecommendedForTipoCapo(commessa.TipoCapo, x.Item.TipoMisura);
                    return x.Item;
                })
                .ToList();

            var free = misureConStatoLink
                .Where(x => !x.IsLinked)
                .Select(x =>
                {
                    x.Item.IsRecommended = CommessaAccessHelper.IsMeasurementRecommendedForTipoCapo(commessa.TipoCapo, x.Item.TipoMisura);
                    return x.Item;
                })
                .ToList();

            var totalMisureCliente = linked.Count + free.Count;

            var misuraStatus = new CommessaMisuraStatus
            {
                HasMisureCollegate = linked.Count > 0,
                HasMisureDisponibili = free.Count > 0,
                RequireMisuraCreation = totalMisureCliente == 0,
                TotaleMisureCliente = totalMisureCliente
            };

            var measurementTypes = await GetActiveMeasurementTypesAsync();

            return new CommessaDetailsViewModel
            {
                Commessa = commessa,
                StatiDisponibili = CommessaAccessHelper.GetAllowedNextStates(commessa.Stato),
                MisureCollegate = linked,
                MisureDisponibili = free,
                HasLinkedMeasureTypeMismatch = linked.Count > 0 && !linked.Any(x => x.IsRecommended),
                MisuraStatus = misuraStatus,
                MeasurementTypes = measurementTypes
            };
        }

        public async Task<List<CommessaMisuraItem>> GetMisureDisponibiliPerClienteAsync(
            int clienteId, int? negozioId, bool isAdmin)
        {
            if (!isAdmin)
            {
                if (!negozioId.HasValue) return new List<CommessaMisuraItem>();

                var hasClienteAccess = await _context.Clienti
                    .AsNoTracking()
                    .AnyAsync(c => c.Id == clienteId && c.NegozioId == negozioId.Value);
                if (!hasClienteAccess) return new List<CommessaMisuraItem>();
            }

            return await _context.Misure
                .AsNoTracking()
                .Where(m => m.ClienteId == clienteId)
                .OrderByDescending(m => m.DataCreazione)
                .Select(m => new CommessaMisuraItem
                {
                    MisuraClienteId = m.Id,
                    RecordId = m.RecordId,
                    TipoMisura = m.TipoMisura,
                    IsDynamic = m.IsDynamic,
                    DataCreazione = m.DataCreazione,
                    Note = m.Note ?? m.SystemNote
                })
                .ToListAsync();
        }

        public async Task<CommessaMisuraStatus> GetStatoMisureClienteAsync(
            int commessaId, int? negozioId, bool isAdmin)
        {
            var (commessa, _) = await CommessaAccessHelper.FetchAndAuthorizeAsync(
                _context.Commissioni
                    .AsNoTracking()
                    .Select(c => new { c.Id, c.ClienteId, c.NegozioId })
                    .Where(c => c.Id == commessaId),
                c => c.NegozioId, negozioId, isAdmin);

            if (commessa is null) return new CommessaMisuraStatus();

            var totaleMisureCliente = await _context.Misure
                .CountAsync(m => m.ClienteId == commessa.ClienteId);
            var linkedCount = await _context.CommissioniMisureLinks
                .CountAsync(x => x.CommessaSartorialeId == commessaId);

            return new CommessaMisuraStatus
            {
                HasMisureCollegate = linkedCount > 0,
                HasMisureDisponibili = totaleMisureCliente > linkedCount,
                RequireMisuraCreation = totaleMisureCliente == 0,
                TotaleMisureCliente = totaleMisureCliente
            };
        }

        internal async Task<List<MeasurementType>> GetActiveMeasurementTypesAsync()
        {
            return await _cache.GetOrCreateAsync(CacheKeys.MeasurementTypesActive, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _context.DynamicMeasurementTypes
                    .AsNoTracking()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Nome)
                    .ToListAsync();
            }) ?? new List<MeasurementType>();
        }
    }
}
