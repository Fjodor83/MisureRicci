using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Services;
using System.Text;
using System.Threading.Tasks;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [Route("Report")]
    public class ReportController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly IMeasurementService _measurementService;

        public ReportController(IClienteService clienteService, IMeasurementService measurementService)
        {
            _clienteService = clienteService;
            _measurementService = measurementService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("ExportClienti")]
        public async Task<IActionResult> ExportClientiCsv()
        {
            var result = await _clienteService.GetClientiPagedAsync("", null, true, 1, 100000);
            
            var builder = new StringBuilder();
            builder.AppendLine("CodiceCliente,Nome,Cognome,Email,Telefono,Città,Paese,DataRegistrazione");
            
            foreach (var c in result.Items)
            {
                builder.AppendLine($"{c.ClientCode},{c.Nome},{c.Cognome},{c.Email},{c.Telefono},{c.Citta},{c.Paese},{c.DataRegistrazione:yyyy-MM-dd}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "ClientiExport.csv");
        }

        [HttpGet("ExportMisure")]
        public async Task<IActionResult> ExportMisureCsv()
        {
            var result = await _measurementService.GetGlobalRegistryAsync("", null, true);
            
            var builder = new StringBuilder();
            builder.AppendLine("DataCreazione,Cliente,CodiceCliente,TipoMisura,Note");
            
            foreach (var m in result)
            {
                builder.AppendLine($"{m.DataCreazione:yyyy-MM-dd HH:mm},{m.Cliente?.Nome} {m.Cliente?.Cognome},{m.Cliente?.ClientCode},{m.TipoMisura},\"{m.Note?.Replace("\"", "\"\"")}\"");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "MisureStoricoExport.csv");
        }
    }
}
