using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public class NegozioService : INegozioService
    {
        private readonly ApplicationDbContext _context;

        public NegozioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Negozio>> GetAllAsync()
        {
            return await _context.Negozi
                .AsNoTracking()
                .OrderBy(x => x.Nome)
                .ToListAsync();
        }

        public async Task<Negozio?> GetByIdAsync(int id)
        {
            return await _context.Negozi.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Negozio> CreateAsync(Negozio negozio)
        {
            _context.Negozi.Add(negozio);
            await _context.SaveChangesAsync();
            return negozio;
        }

        public async Task UpdateAsync(Negozio negozio)
        {
            _context.Negozi.Update(negozio);
            await _context.SaveChangesAsync();
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
        }

        public bool Exists(int id)
        {
            return _context.Negozi.Any(x => x.Id == id);
        }
    }
}