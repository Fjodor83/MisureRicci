using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ApplicationDbContext _context;

        public ClienteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Cliente> Items, int TotalCount)> GetClientiPagedAsync(string searchString, int? negozioId, bool isAdmin, int page, int pageSize)
        {
            var query = _context.Clienti.AsQueryable();

            if (!isAdmin && negozioId.HasValue)
            {
                query = query.Where(c => c.NegozioId == negozioId.Value);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Nome.Contains(searchString) || s.Cognome.Contains(searchString) || (s.ClientCode ?? string.Empty).Contains(searchString));
            }

            var totalCount = await query.CountAsync();
            var clienti = await query
                .OrderByDescending(c => c.DataRegistrazione)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (clienti, totalCount);
        }

        public async Task<List<Cliente>> SearchClientiAsync(string? search, int limit = 50)
        {
            var query = _context.Clienti
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Nome.Contains(search) || c.Cognome.Contains(search) || (c.ClientCode ?? string.Empty).Contains(search));
            }

            return await query
                .OrderBy(c => c.Cognome)
                .ThenBy(c => c.Nome)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Cliente?> GetClienteByIdAsync(int id)
        {
            return await _context.Clienti.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<MisureCliente>> GetStoricoMisureAsync(int clienteId)
        {
            return await _context.RegistroMisure
                .Where(m => m.ClienteId == clienteId)
                .OrderByDescending(m => m.DataCreazione)
                .ToListAsync();
        }

        public async Task<Cliente> CreateClienteAsync(Cliente cliente)
        {
            _context.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task UpdateClienteAsync(Cliente cliente)
        {
            _context.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClienteAsync(int id)
        {
            var cliente = await _context.Clienti.FindAsync(id);
            if (cliente != null)
            {
                _context.Clienti.Remove(cliente);
                await _context.SaveChangesAsync();
            }
        }

        public bool ClienteExists(int id)
        {
            return _context.Clienti.Any(e => e.Id == id);
        }
    }
}
