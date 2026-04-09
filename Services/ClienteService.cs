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

        public async Task<(IEnumerable<Cliente> Items, int TotalCount)> GetClientiPagedAsync(
            string? searchString, int? negozioId, bool isAdmin, int page, int pageSize)
        {
            var query = ApplyClienteScope(_context.Clienti.AsNoTracking(), negozioId, isAdmin);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(s =>
                    s.Nome.Contains(searchString) ||
                    s.Cognome.Contains(searchString) ||
                    (s.ClientCode ?? string.Empty).Contains(searchString));
            }

            var totalCount = await query.CountAsync();
            var clienti = await query
                .OrderByDescending(c => c.DataRegistrazione)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (clienti, totalCount);
        }

        public async Task<List<Cliente>> SearchClientiAsync(
            string? search, int? negozioId, bool isAdmin, int limit = 50)
        {
            var query = _context.Clienti.AsNoTracking().AsQueryable();

            if (!isAdmin)
            {
                if (!negozioId.HasValue) return new List<Cliente>();
                query = query.Where(c => c.NegozioId == negozioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(c =>
                    EF.Functions.Like(c.Nome, $"%{search}%") ||
                    EF.Functions.Like(c.Cognome, $"%{search}%") ||
                    EF.Functions.Like(c.ClientCode ?? string.Empty, $"%{search}%"));
            }

            return await query
                .OrderBy(c => c.Cognome)
                .ThenBy(c => c.Nome)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Cliente?> GetClienteByIdAsync(int id)
        {
            return await _context.Clienti.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<MisureCliente>> GetStoricoMisureAsync(int clienteId)
        {
            return await _context.Misure
                .AsNoTracking()
                .Where(m => m.ClienteId == clienteId)
                .OrderByDescending(m => m.DataCreazione)
                .ToListAsync();
        }

        public async Task<Cliente?> GetClienteScopedAsync(int id, int? negozioId, bool isAdmin)
        {
            return await ApplyClienteScope(_context.Clienti.AsNoTracking(), negozioId, isAdmin)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<MisureCliente>> GetStoricoMisureScopedAsync(
            int clienteId, int? negozioId, bool isAdmin)
        {
            return await ApplyStoricoScope(
                    _context.Misure.AsNoTracking().Include(m => m.Cliente),
                    negozioId,
                    isAdmin)
                .Where(m => m.ClienteId == clienteId)
                .OrderByDescending(m => m.DataCreazione)
                .ToListAsync();
        }

        public async Task<Result<Cliente>> CreateClienteScopedAsync(Cliente cliente, int? negozioId, bool isAdmin)
        {
            if (!CanAccessTenant(negozioId, isAdmin))
                return Result<Cliente>.Fail("Accesso negato: tenant non assegnato.");

            NormalizeCliente(cliente);

            if (isAdmin)
            {
                if (!cliente.NegozioId.HasValue)
                    return Result<Cliente>.Fail("Il negozio è obbligatorio.");
            }
            else
            {
                cliente.NegozioId = negozioId;
            }

            _context.Clienti.Add(cliente);
            await _context.SaveChangesAsync();

            // Genera ClientCode dopo aver ottenuto l'Id (simulazione colonna calcolata)
            cliente.ClientCode = GenerateClientCode(cliente.Id, cliente.DataRegistrazione);
            await _context.SaveChangesAsync();

            return Result<Cliente>.Ok(cliente);
        }

        public async Task<Result> UpdateClienteScopedAsync(Cliente cliente, int? negozioId, bool isAdmin)
        {
            if (!CanAccessTenant(negozioId, isAdmin))
                return Result.Fail("Accesso negato: tenant non assegnato.");

            var existing = await ApplyClienteScope(_context.Clienti, negozioId, isAdmin)
                .FirstOrDefaultAsync(c => c.Id == cliente.Id);

            if (existing == null) return Result.Fail("Cliente non trovato.");

            NormalizeCliente(cliente);

            existing.Nome = cliente.Nome;
            existing.Cognome = cliente.Cognome;
            existing.Email = cliente.Email;
            existing.Telefono = cliente.Telefono;
            existing.Indirizzo = cliente.Indirizzo;
            existing.Citta = cliente.Citta;
            existing.StatoProvincia = cliente.StatoProvincia;
            existing.CodicePostale = cliente.CodicePostale;
            existing.Paese = cliente.Paese;
            existing.Note = cliente.Note;

            if (isAdmin)
            {
                if (!cliente.NegozioId.HasValue)
                    return Result.Fail("Il negozio è obbligatorio.");
                existing.NegozioId = cliente.NegozioId;
            }

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> DeleteClienteScopedAsync(int id, int? negozioId, bool isAdmin)
        {
            if (!CanAccessTenant(negozioId, isAdmin))
                return Result.Fail("Accesso negato: tenant non assegnato.");

            var cliente = await ApplyClienteScope(_context.Clienti, negozioId, isAdmin)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null) return Result.Fail("Cliente non trovato.");

            _context.Clienti.Remove(cliente);
            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Cliente> CreateClienteAsync(Cliente cliente)
        {
            NormalizeCliente(cliente);
            _context.Clienti.Add(cliente);
            await _context.SaveChangesAsync();

            cliente.ClientCode = GenerateClientCode(cliente.Id, cliente.DataRegistrazione);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task UpdateClienteAsync(Cliente cliente)
        {
            NormalizeCliente(cliente);
            var existing = await _context.Clienti.FindAsync(cliente.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(cliente);
                // Preserva ClientCode generato
                _context.Entry(existing).Property(x => x.ClientCode).IsModified = false;
                await _context.SaveChangesAsync();
            }
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

        public bool ClienteExists(int id) => _context.Clienti.Any(e => e.Id == id);

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Genera ClientCode nel formato SR-YYYY-NNNNN,
        /// replicando la logica della colonna calcolata SQL Server.
        /// </summary>
        private static string GenerateClientCode(int id, DateTime dataRegistrazione)
        {
            return $"SR-{dataRegistrazione.Year}-{id:D5}";
        }

        private static bool CanAccessTenant(int? negozioId, bool isAdmin)
            => isAdmin || negozioId.HasValue;

        private static IQueryable<Cliente> ApplyClienteScope(
            IQueryable<Cliente> query, int? negozioId, bool isAdmin)
        {
            if (isAdmin) return query;
            if (!negozioId.HasValue) return query.Where(_ => false);
            return query.Where(c => c.NegozioId == negozioId.Value);
        }

        private static IQueryable<MisureCliente> ApplyStoricoScope(
            IQueryable<MisureCliente> query, int? negozioId, bool isAdmin)
        {
            if (isAdmin) return query;
            if (!negozioId.HasValue) return query.Where(_ => false);
            return query.Where(m => m.Cliente != null && m.Cliente.NegozioId == negozioId.Value);
        }

        private static void NormalizeCliente(Cliente cliente)
        {
            cliente.Nome = cliente.Nome?.Trim() ?? string.Empty;
            cliente.Cognome = cliente.Cognome?.Trim() ?? string.Empty;
            cliente.Email = cliente.Email?.Trim() ?? string.Empty;
            cliente.Telefono = NormalizeNullable(cliente.Telefono);
            cliente.Indirizzo = NormalizeNullable(cliente.Indirizzo);
            cliente.Citta = NormalizeNullable(cliente.Citta);
            cliente.StatoProvincia = NormalizeNullable(cliente.StatoProvincia);
            cliente.CodicePostale = NormalizeNullable(cliente.CodicePostale);
            cliente.Paese = string.IsNullOrWhiteSpace(cliente.Paese) ? "Italy" : cliente.Paese.Trim();
            cliente.Note = NormalizeNullable(cliente.Note);
        }

        private static string? NormalizeNullable(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}