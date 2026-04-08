using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MisureRicci.Services;

namespace MisureRicci.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("api")]
    public class ClientiApiController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ITenantService _tenantService;

        public ClientiApiController(IClienteService clienteService, ITenantService tenantService)
        {
            _clienteService = clienteService;
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetClienti([FromQuery] string? search)
        {
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();
            
            var clienti = await _clienteService.SearchClientiAsync(search, currentNegozioId, isAdmin, limit: 100);

            return Ok(clienti.Select(c => new { c.Id, c.ClientCode, c.Nome, c.Cognome }));
        }

        [HttpGet("{id}/misure")]
        public async Task<IActionResult> GetMisure(int id)
        {
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();

            var cliente = await _clienteService.GetClienteScopedAsync(id, currentNegozioId, isAdmin);
            if (cliente == null)
            {
                return NotFound();
            }

            var misure = await _clienteService.GetStoricoMisureScopedAsync(id, currentNegozioId, isAdmin);

            return Ok(misure);
        }
    }
}
