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
            var tenantKey = isAdmin ? "global" : (negozioId?.ToString() ?? "unknown");
            var cacheKey = $"{DashboardKpiCacheKey}_{tenantKey}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

                var clientsQuery = _context.Clienti.AsQueryable();
                var measurementsQuery = _context.Misure.AsQueryable();
                var storesQuery = _context.Negozi.AsQueryable();
                var staffQuery = _context.Users.AsQueryable();

                if (!isAdmin)
                {
                    if (negozioId.HasValue)
                    {
                        clientsQuery = clientsQuery.Where(x => x.NegozioId == negozioId.Value);
                        measurementsQuery = measurementsQuery.Where(x => x.Cliente!.NegozioId == negozioId.Value);
                        // Stores: depends on requirement, but usually staff sees only their store
                        storesQuery = storesQuery.Where(x => x.Id == negozioId.Value);
                        staffQuery = staffQuery.Where(x => x.NegozioId == negozioId.Value);
                    }
                    else
                    {
                        // No access if not admin and no store assigned
                        return new DashboardKpiViewModel();
                    }
                }

                return new DashboardKpiViewModel
                {
                    TotalClients = await clientsQuery.CountAsync(),
                    TotalMeasurements = await measurementsQuery.CountAsync(),
                    TotalStores = await storesQuery.CountAsync(),
                    TotalStaff = await staffQuery.CountAsync()
                };
            }) ?? new DashboardKpiViewModel();
        }
    }
}
