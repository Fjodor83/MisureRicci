using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;

namespace MisureRicci.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientiApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientiApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetClienti([FromQuery] string? search)
        {
            var q = _context.Clienti.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                q = q.Where(c => c.Nome.Contains(search) || c.Cognome.Contains(search));
                
            return Ok(await q.Select(c => new { c.Id, c.ClientCode, c.Nome, c.Cognome }).ToListAsync());
        }

        [HttpGet("{id}/misure")]
        public async Task<IActionResult> GetMisure(int id)
        {
            var misure = await _context.RegistroMisure
                .Where(m => m.ClienteId == id)
                .OrderByDescending(m => m.DataCreazione)
                .ToListAsync();

            return Ok(misure);
        }
    }
}
