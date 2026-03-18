using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public PdfController(IPdfService pdfService, IClienteService clienteService)
        {
            _pdfService = pdfService;
            _clienteService = clienteService;
        }

        [HttpGet("dossier/{clienteId}")]
        public async Task<IActionResult> DossierCliente(int clienteId)
        {
            var bytes = await _pdfService.GenerateDossierPdfAsync(clienteId);
            if (bytes == null || bytes.Length == 0) return NotFound();
            
            var cliente = await _clienteService.GetClienteByIdAsync(clienteId);
            
            return File(bytes, "application/pdf", $"dossier-{cliente?.ClientCode ?? "cliente"}.pdf");
        }
    }
}
