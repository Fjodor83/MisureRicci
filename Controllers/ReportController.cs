using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using System.Text;
using System.Threading.Tasks;

namespace MisureRicci.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [Route("Report")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("ExportClienti")]
        public async Task<IActionResult> ExportClientiCsv(CancellationToken ct)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Forbid();
            }

            var isAdmin = User.IsInRole("Admin");
            var negozioId = isAdmin ? (int?)null : currentUser.NegozioId;

            Response.ContentType = "text/csv; charset=utf-8";
            Response.Headers["Content-Disposition"] = "attachment; filename=ClientiExport.csv";

            await using var writer = new StreamWriter(Response.Body, new UTF8Encoding(false), leaveOpen: true);
            await writer.WriteLineAsync("CodiceCliente,Nome,Cognome,Email,Telefono,Città,Paese,DataRegistrazione");

            var query = _context.Clienti
                .AsNoTracking()
                .AsQueryable();

            if (!isAdmin)
            {
                if (!negozioId.HasValue)
                {
                    await writer.FlushAsync(ct);
                    return new EmptyResult();
                }

                query = query.Where(c => c.NegozioId == negozioId.Value);
            }

            var rowCount = 0;
            await foreach (var c in query.OrderBy(c => c.Id).AsAsyncEnumerable().WithCancellation(ct))
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
                if (rowCount % 500 == 0)
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
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Forbid();
            }

            var isAdmin = User.IsInRole("Admin");
            var negozioId = isAdmin ? (int?)null : currentUser.NegozioId;

            Response.ContentType = "text/csv; charset=utf-8";
            Response.Headers["Content-Disposition"] = "attachment; filename=MisureStoricoExport.csv";

            await using var writer = new StreamWriter(Response.Body, new UTF8Encoding(false), leaveOpen: true);
            await writer.WriteLineAsync("DataCreazione,Cliente,CodiceCliente,TipoMisura,Note");

            var query = _context.RegistroMisure
                .AsNoTracking()
                .Include(m => m.Cliente)
                .AsQueryable();

            if (!isAdmin)
            {
                if (!negozioId.HasValue)
                {
                    await writer.FlushAsync(ct);
                    return new EmptyResult();
                }

                query = query.Where(m => m.Cliente != null && m.Cliente.NegozioId == negozioId.Value);
            }

            var rowCount = 0;
            await foreach (var m in query.OrderByDescending(m => m.DataCreazione).AsAsyncEnumerable().WithCancellation(ct))
            {
                await writer.WriteLineAsync(string.Join(",",
                    SanitizeCsvField(m.DataCreazione.ToString("yyyy-MM-dd HH:mm")),
                    SanitizeCsvField($"{m.Cliente?.Nome} {m.Cliente?.Cognome}".Trim()),
                    SanitizeCsvField(m.Cliente?.ClientCode),
                    SanitizeCsvField(m.TipoMisura),
                    SanitizeCsvField(m.Note ?? m.SystemNote)));

                rowCount++;
                if (rowCount % 500 == 0)
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
