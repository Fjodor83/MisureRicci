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

                var totalClients = await _context.Clienti.CountAsync();
                var totalMeasurements = await _context.Misure.CountAsync();
                var totalStores = await _context.Negozi.CountAsync();
                var totalStaff = await _context.Users.CountAsync();

                return new DashboardKpiViewModel
                {
                    TotalClients = totalClients,
                    TotalMeasurements = totalMeasurements,
                    TotalStores = totalStores,
                    TotalStaff = totalStaff
                };
            }) ?? new DashboardKpiViewModel();
        }
    }
}
