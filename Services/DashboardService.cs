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

                Task<int> CountFilteredAsync<TEntity>(
                    Func<ApplicationDbContext, IQueryable<TEntity>> baseQuery)
                    where TEntity : class
                {
                    return CountAsync((ctx, token) =>
                    {
                        var q = baseQuery(ctx);
                        if (!isAdmin)
                            q = q.Where(e => EF.Property<int>(e, "NegozioId") == storeId);
                        return q.CountAsync(token);
                    }, ct);
                }

                var tasks = await Task.WhenAll(
                    CountFilteredAsync(ctx => ctx.Clienti),
                    CountFilteredAsync(ctx => ctx.Misure),
                    CountFilteredAsync(ctx => ctx.Negozi),
                    CountFilteredAsync(ctx => ctx.Users)
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
    }
}
