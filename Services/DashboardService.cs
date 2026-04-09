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
            var tenantKey = isAdmin ? "global" : (negozioId?.ToString() ?? "unknown");
            var cacheKey = $"{CacheKeys.DashboardKpiPrefix}{tenantKey}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

                if (!isAdmin && !negozioId.HasValue)
                    return new DashboardKpiViewModel();

                var tasks = await Task.WhenAll(
                    CountAsync((ctx, token) =>
                    {
                        var q = ctx.Clienti.AsQueryable();
                        if (!isAdmin) q = q.Where(x => x.NegozioId == negozioId!.Value);
                        return q.CountAsync(token);
                    }, ct),
                    CountAsync((ctx, token) =>
                    {
                        var q = ctx.Misure.AsQueryable();
                        if (!isAdmin) q = q.Where(x => x.Cliente!.NegozioId == negozioId!.Value);
                        return q.CountAsync(token);
                    }, ct),
                    CountAsync((ctx, token) =>
                    {
                        var q = ctx.Negozi.AsQueryable();
                        if (!isAdmin) q = q.Where(x => x.Id == negozioId!.Value);
                        return q.CountAsync(token);
                    }, ct),
                    CountAsync((ctx, token) =>
                    {
                        var q = ctx.Users.AsQueryable();
                        if (!isAdmin) q = q.Where(x => x.NegozioId == negozioId!.Value);
                        return q.CountAsync(token);
                    }, ct));

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
