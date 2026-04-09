using MisureRicci.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MisureRicci.Services
{
    public interface IClienteService
    {
        Task<(IEnumerable<Cliente> Items, int TotalCount)> GetClientiPagedAsync(string? searchString, int? negozioId, bool isAdmin, int page, int pageSize);
        Task<List<Cliente>> SearchClientiAsync(string? search, int? negozioId, bool isAdmin, int limit = 50);
        Task<Cliente?> GetClienteByIdAsync(int id);
        Task<List<MisureCliente>> GetStoricoMisureAsync(int clienteId);
        Task<Cliente?> GetClienteScopedAsync(int id, int? negozioId, bool isAdmin);
        Task<List<MisureCliente>> GetStoricoMisureScopedAsync(int clienteId, int? negozioId, bool isAdmin);
        Task<Result<Cliente>> CreateClienteScopedAsync(Cliente cliente, int? negozioId, bool isAdmin);
        Task<Result> UpdateClienteScopedAsync(Cliente cliente, int? negozioId, bool isAdmin);
        Task<Result> DeleteClienteScopedAsync(int id, int? negozioId, bool isAdmin);
        Task<Cliente> CreateClienteAsync(Cliente cliente);
        Task UpdateClienteAsync(Cliente cliente);
        Task DeleteClienteAsync(int id);
        bool ClienteExists(int id);
    }
}
