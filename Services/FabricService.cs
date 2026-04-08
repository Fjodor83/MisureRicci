using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public class FabricService : IFabricService
    {
        private readonly ApplicationDbContext _context;

        public FabricService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Fabric>> GetFabricsAsync(bool onlyActive = true)
        {
            var query = _context.Fabrics.AsNoTracking();
            if (onlyActive)
                query = query.Where(f => f.IsActive);
            
            return await query.OrderBy(f => f.Nome).ToListAsync();
        }

        public async Task<Fabric?> GetFabricByIdAsync(int id)
        {
            return await _context.Fabrics.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Fabric> CreateFabricAsync(Fabric model)
        {
            _context.Fabrics.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task UpdateFabricAsync(Fabric model)
        {
            var existing = await _context.Fabrics.FirstOrDefaultAsync(f => f.Id == model.Id);
            if (existing == null) throw new InvalidOperationException("Fabric not found");

            existing.Nome = model.Nome;
            existing.Descrizione = model.Descrizione;
            existing.Composizione = model.Composizione;
            existing.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteFabricAsync(int id)
        {
            var fabric = await _context.Fabrics.FirstOrDefaultAsync(f => f.Id == id);
            if (fabric != null)
            {
                _context.Fabrics.Remove(fabric);
                await _context.SaveChangesAsync();
            }
        }
    }
}
