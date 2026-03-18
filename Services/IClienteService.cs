using MisureRicci.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MisureRicci.Services
{
    public interface IClienteService
    {
        Task<(IEnumerable<Cliente> Items, int TotalCount)> GetClientiPagedAsync(string searchString, int? negozioId, bool isAdmin, int page, int pageSize);
        Task<Cliente?> GetClienteByIdAsync(int id);
        Task<List<MisureCliente>> GetStoricoMisureAsync(int clienteId);
        Task<Cliente> CreateClienteAsync(Cliente cliente);
        Task UpdateClienteAsync(Cliente cliente);
        Task DeleteClienteAsync(int id);
        bool ClienteExists(int id);
    }
}
