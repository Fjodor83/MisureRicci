using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Services;

namespace MisureRicci.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientiApiController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientiApiController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetClienti([FromQuery] string? search)
        {
            var clienti = await _clienteService.SearchClientiAsync(search, limit: 100);

            return Ok(clienti.Select(c => new { c.Id, c.ClientCode, c.Nome, c.Cognome }));
        }

        [HttpGet("{id}/misure")]
        public async Task<IActionResult> GetMisure(int id)
        {
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            var misure = await _clienteService.GetStoricoMisureAsync(id);

            return Ok(misure);
        }
    }
}
