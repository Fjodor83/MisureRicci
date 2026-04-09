using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    public class ReportController : TenantAwareController
    {
        private readonly ApplicationDbContext _context;

        public ReportController(
            ApplicationDbContext context,
            ITenantService tenantService)
            : base(tenantService)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var tenantCheck = RequireTenant();
            if (tenantCheck != null) return tenantCheck;

            return View();
        }

        [HttpGet("ExportClienti")]
        [EnableRateLimiting("export")]
        public async Task<IActionResult> ExportClientiCsv(CancellationToken ct)
        {
            if (!IsAdmin && !NegozioId.HasValue)
            {
                return Forbid();
            }

            Response.ContentType = "text/csv; charset=utf-8";
            Response.Headers["Content-Disposition"] = "attachment; filename=ClientiExport.csv";

            await using var writer = new StreamWriter(Response.Body, new UTF8Encoding(false), leaveOpen: true);
            await writer.WriteLineAsync("CodiceCliente,Nome,Cognome,Email,Telefono,Città,Paese,DataRegistrazione");

            var query = _context.Clienti
                .AsNoTracking()
                .OrderBy(c => c.Id);

            if (!IsAdmin)
            {
                query = query
                        .Where(c => c.NegozioId == NegozioId!.Value)
                    .OrderBy(c => c.Id);
            }

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
        [EnableRateLimiting("export")]
        public async Task<IActionResult> ExportMisureCsv(CancellationToken ct)
        {
            if (!IsAdmin && !NegozioId.HasValue)
            {
                return Forbid();
            }

            Response.ContentType = "text/csv; charset=utf-8";
            Response.Headers["Content-Disposition"] = "attachment; filename=MisureStoricoExport.csv";

            await using var writer = new StreamWriter(Response.Body, new UTF8Encoding(false), leaveOpen: true);
            await writer.WriteLineAsync("DataCreazione,Cliente,CodiceCliente,TipoMisura,Note");

            var query = _context.Misure
                .AsNoTracking()
                .Include(m => m.Cliente)
                .OrderByDescending(m => m.DataCreazione);

            if (!IsAdmin)
            {
                query = query
                        .Where(m => m.Cliente != null && m.Cliente.NegozioId == NegozioId!.Value)
                    .OrderByDescending(m => m.DataCreazione);
            }

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
