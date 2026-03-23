using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public class UtenteService : IUtenteService
    {
        private readonly ApplicationDbContext _context;

        public UtenteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Utente>> GetAllAsync()
        {
            return await _context.Utenti
                .AsNoTracking()
                .Include(u => u.Negozio)
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();
        }

        public async Task<Utente?> GetByIdAsync(int id)
        {
            return await _context.Utenti
                .Include(u => u.Negozio)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Utente> CreateAsync(Utente utente)
        {
            _context.Utenti.Add(utente);
            await _context.SaveChangesAsync();
            return utente;
        }

        public async Task UpdateAsync(Utente utente)
        {
            _context.Utenti.Update(utente);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Utenti.FindAsync(id);
            if (entity == null)
            {
                return;
            }

            _context.Utenti.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public bool Exists(int id)
        {
            return _context.Utenti.Any(x => x.Id == id);
        }
    }
}