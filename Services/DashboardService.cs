using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MisureRicci.Data;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public class DashboardService : IDashboardService
    {
        private const string DashboardKpiCacheKey = "dashboard_kpi_v1";
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public DashboardService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<DashboardKpiViewModel> GetKpiAsync(int? negozioId, bool isAdmin)
        {
            var cacheKey = isAdmin
                ? DashboardKpiCacheKey
                : $"{DashboardKpiCacheKey}_negozio_{negozioId?.ToString() ?? "none"}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                if (!isAdmin && !negozioId.HasValue)
                {
                    return new DashboardKpiViewModel();
                }

                var scopedNegozioId = negozioId.GetValueOrDefault();
                var clientiQuery = _context.Clienti.AsNoTracking().AsQueryable();
                var utentiQuery = _context.Users.AsNoTracking().AsQueryable();
                var misureQuery = _context.RegistroMisure.AsNoTracking().AsQueryable();
                var negoziQuery = _context.Negozi.AsNoTracking().AsQueryable();

                if (!isAdmin)
                {
                    clientiQuery = clientiQuery.Where(c => c.NegozioId == scopedNegozioId);
                    utentiQuery = utentiQuery.Where(u => u.NegozioId == scopedNegozioId);
                    misureQuery = misureQuery.Where(m => m.Cliente != null && m.Cliente.NegozioId == scopedNegozioId);
                    negoziQuery = negoziQuery.Where(n => n.Id == scopedNegozioId);
                }

                return new DashboardKpiViewModel
                {
                    TotalClients = await clientiQuery.CountAsync(),
                    TotalStores = await negoziQuery.CountAsync(),
                    TotalStaff = await utentiQuery.CountAsync(),
                    TotalMeasurements = await misureQuery.CountAsync()
                };
            }) ?? new DashboardKpiViewModel();
        }
    }
}
