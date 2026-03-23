using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public class NegozioService : INegozioService
    {
        private const string NegoziCacheKey = "negozi_all_v1";
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public NegozioService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Negozio>> GetAllAsync()
        {
            return await _cache.GetOrCreateAsync(NegoziCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _context.Negozi
                    .AsNoTracking()
                    .OrderBy(x => x.Nome)
                    .ToListAsync();
            }) ?? new List<Negozio>();
        }

        public async Task<Negozio?> GetByIdAsync(int id)
        {
            return await _context.Negozi.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Negozio> CreateAsync(Negozio negozio)
        {
            _context.Negozi.Add(negozio);
            await _context.SaveChangesAsync();
            _cache.Remove(NegoziCacheKey);
            return negozio;
        }

        public async Task UpdateAsync(Negozio negozio)
        {
            _context.Negozi.Update(negozio);
            await _context.SaveChangesAsync();
            _cache.Remove(NegoziCacheKey);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Negozi.FindAsync(id);
            if (entity == null)
            {
                return;
            }

            _context.Negozi.Remove(entity);
            await _context.SaveChangesAsync();
            _cache.Remove(NegoziCacheKey);
        }

        public bool Exists(int id)
        {
            return _context.Negozi.Any(x => x.Id == id);
        }
    }
}