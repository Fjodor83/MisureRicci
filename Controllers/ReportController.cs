using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Services;
using System.Text;
using System.Threading.Tasks;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin,Manager,Sartoria,Boutique")]
    [Route("Report")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        public ReportController(
            ApplicationDbContext context,
            ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();

            if (!isAdmin && !currentNegozioId.HasValue)
            {
                return Forbid();
            }

            return View();
        }

        [HttpGet("ExportClienti")]
        public async Task<IActionResult> ExportClientiCsv(CancellationToken ct)
        {
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();

            if (!isAdmin && !currentNegozioId.HasValue)
            {
                return Forbid();
            }

            Response.ContentType = "text/csv; charset=utf-8";
            Response.Headers["Content-Disposition"] = "attachment; filename=ClientiExport.csv";

            await using var writer = new StreamWriter(Response.Body, new UTF8Encoding(false), leaveOpen: true);
            await writer.WriteLineAsync("CodiceCliente,Nome,Cognome,Email,Telefono,Città,Paese,DataRegistrazione");

            // Global Query Filters handle tenant isolation
            var query = _context.Clienti
                .AsNoTracking()
                .OrderBy(c => c.Id);

            var rowCount = 0;
            await foreach (var c in query.AsAsyncEnumerable().WithCancellation(ct))
            {
                await writer.WriteLineAsync(string.Join(",",
                    SanitizeCsvField(c.ClientCode),
                    SanitizeCsvField(c.Nome),
                    SanitizeCsvField(c.Cognome),
                    SanitizeCsvField(c.Email),
                    SanitizeCsvField(c.Telefono),
                    SanitizeCsvField(c.Citta),
                    SanitizeCsvField(c.Paese),
                    SanitizeCsvField(c.DataRegistrazione.ToString("yyyy-MM-dd"))));

                rowCount++;
                if (rowCount % 100 == 0) // Frequent flush for streaming
                {
                    await writer.FlushAsync(ct);
                }
            }

            await writer.FlushAsync(ct);
            return new EmptyResult();
        }

        [HttpGet("ExportMisure")]
        public async Task<IActionResult> ExportMisureCsv(CancellationToken ct)
        {
            var isAdmin = _tenantService.IsAdmin();
            var currentNegozioId = _tenantService.GetCurrentNegozioId();

            if (!isAdmin && !currentNegozioId.HasValue)
            {
                return Forbid();
            }

            Response.ContentType = "text/csv; charset=utf-8";
            Response.Headers["Content-Disposition"] = "attachment; filename=MisureStoricoExport.csv";

            await using var writer = new StreamWriter(Response.Body, new UTF8Encoding(false), leaveOpen: true);
            await writer.WriteLineAsync("DataCreazione,Cliente,CodiceCliente,TipoMisura,Note");

            // Global Query Filters handle tenant isolation
            var query = _context.Misure
                .AsNoTracking()
                .Include(m => m.Cliente)
                .OrderByDescending(m => m.DataCreazione);

            var rowCount = 0;
            await foreach (var m in query.AsAsyncEnumerable().WithCancellation(ct))
            {
                await writer.WriteLineAsync(string.Join(",",
                    SanitizeCsvField(m.DataCreazione.ToString("yyyy-MM-dd HH:mm")),
                    SanitizeCsvField($"{m.Cliente?.Nome} {m.Cliente?.Cognome}".Trim()),
                    SanitizeCsvField(m.Cliente?.ClientCode),
                    SanitizeCsvField(m.TipoMisura),
                    SanitizeCsvField(m.Note ?? m.SystemNote)));

                rowCount++;
                if (rowCount % 100 == 0)
                {
                    await writer.FlushAsync(ct);
                }
            }

            await writer.FlushAsync(ct);
            return new EmptyResult();
        }

        private static string SanitizeCsvField(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            var sanitized = value.Replace("\"", "\"\"");
            var firstChar = sanitized[0];
            if (firstChar == '=' || firstChar == '+' || firstChar == '-' || firstChar == '@' || firstChar == '\t' || firstChar == '\r')
            {
                sanitized = "'" + sanitized;
            }

            return $"\"{sanitized}\"";
        }
    }
}
