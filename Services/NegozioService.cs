using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public class NegozioService : INegozioService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public NegozioService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Negozio>> GetAllAsync()
        {
            return await _cache.GetOrCreateAsync(CacheKeys.NegozioAll, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return await _context.Negozi
                    .AsNoTracking()
                    .OrderBy(x => x.Nome)
                    .ToListAsync();
            }) ?? new List<Negozio>();
        }

        public void InvalidateCache()
        {
            _cache.Remove(CacheKeys.NegozioAll);
        }

        public async Task<Negozio?> GetByIdAsync(int id)
        {
            return await _context.Negozi.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Result<Negozio>> CreateAsync(Negozio negozio)
        {
            _context.Negozi.Add(negozio);
            await _context.SaveChangesAsync();
            _cache.Remove(CacheKeys.NegozioAll);
            return Result<Negozio>.Ok(negozio);
        }

        public async Task<Result> UpdateAsync(Negozio negozio)
        {
            var existing = await _context.Negozi.FindAsync(negozio.Id);
            if (existing == null)
                return Result.Fail("Negozio non trovato.");

            _context.Entry(existing).CurrentValues.SetValues(negozio);
            await _context.SaveChangesAsync();
            _cache.Remove(CacheKeys.NegozioAll);
            return Result.Ok();
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var entity = await _context.Negozi.FindAsync(id);
            if (entity == null)
                return Result.Fail("Negozio non trovato.");

            _context.Negozi.Remove(entity);
            await _context.SaveChangesAsync();
            _cache.Remove(CacheKeys.NegozioAll);
            return Result.Ok();
        }

        public bool Exists(int id)
        {
            return _context.Negozi.Any(x => x.Id == id);
        }
    }
}