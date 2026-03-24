using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Models;
using MisureRicci.Services;
using System.Threading.Tasks;

namespace MisureRicci.Controllers
{
    [Authorize]
    [Route("pdf")]
    public class PdfController : Controller
    {
        private readonly IPdfService _pdfService;
        private readonly IClienteService _clienteService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PdfController(IPdfService pdfService, IClienteService clienteService, UserManager<ApplicationUser> userManager)
        {
            _pdfService = pdfService;
            _clienteService = clienteService;
            _userManager = userManager;
        }

        [HttpGet("dossier/{clienteId}")]
        public async Task<IActionResult> DossierCliente(int clienteId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            var cliente = await _clienteService.GetClienteScopedAsync(clienteId, currentUser?.NegozioId, isAdmin);
            if (cliente == null)
            {
                return NotFound();
            }

            byte[] bytes;
            try
            {
                bytes = await _pdfService.GenerateDossierPdfAsync(clienteId, currentUser?.NegozioId, isAdmin);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            if (bytes == null || bytes.Length == 0) return NotFound();

            return File(bytes, "application/pdf", $"dossier-{cliente?.ClientCode ?? "cliente"}.pdf");
        }
    }
}
