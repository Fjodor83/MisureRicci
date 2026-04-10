using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MisureRicci.Data;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;

        public DashboardService(IServiceScopeFactory scopeFactory, IMemoryCache cache)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
        }

        public async Task<DashboardKpiViewModel> GetKpiAsync(int? negozioId, bool isAdmin, CancellationToken ct = default)
        {
            var tenantKey = isAdmin
                ? "global"
                : (negozioId?.ToString() ?? "unknown");

            var cacheKey = $"{CacheKeys.DashboardKpiPrefix}{tenantKey}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

                if (!isAdmin && !negozioId.HasValue)
                    return new DashboardKpiViewModel();

                int storeId = negozioId ?? 0;

                var tasks = await Task.WhenAll(
                    CountClientsAsync(isAdmin, storeId, ct),
                    CountMeasurementsAsync(isAdmin, storeId, ct),
                    CountStoresAsync(isAdmin, storeId, ct),
                    CountStaffAsync(isAdmin, storeId, ct)
                );

                return new DashboardKpiViewModel
                {
                    TotalClients = tasks[0],
                    TotalMeasurements = tasks[1],
                    TotalStores = tasks[2],
                    TotalStaff = tasks[3]
                };
            }) ?? new DashboardKpiViewModel();
        }

        private async Task<int> CountAsync(Func<ApplicationDbContext, CancellationToken, Task<int>> query, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await query(ctx, ct);
        }

        private Task<int> CountClientsAsync(bool isAdmin, int storeId, CancellationToken ct)
            => CountAsync((ctx, token) =>
                (isAdmin
                    ? ctx.Clienti
                    : ctx.Clienti.Where(c => c.NegozioId == storeId))
                .CountAsync(token), ct);

        private Task<int> CountMeasurementsAsync(bool isAdmin, int storeId, CancellationToken ct)
            => CountAsync((ctx, token) =>
                (isAdmin
                    ? ctx.Misure
                    : ctx.Misure.Where(m => m.Cliente != null && m.Cliente.NegozioId == storeId))
                .CountAsync(token), ct);

        private Task<int> CountStoresAsync(bool isAdmin, int storeId, CancellationToken ct)
            => CountAsync((ctx, token) =>
                (isAdmin
                    ? ctx.Negozi
                    : ctx.Negozi.Where(n => n.Id == storeId))
                .CountAsync(token), ct);

        private Task<int> CountStaffAsync(bool isAdmin, int storeId, CancellationToken ct)
            => CountAsync((ctx, token) =>
                (isAdmin
                    ? ctx.Users
                    : ctx.Users.Where(u => u.NegozioId == storeId))
                .CountAsync(token), ct);
    }
}
