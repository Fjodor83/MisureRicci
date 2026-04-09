using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MisureRicci.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MisureRicci.Services
{
    public class PdfService : IPdfService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfStorageService _pdfStorageService;
        private readonly ILogger<PdfService> _logger;

        public PdfService(ApplicationDbContext context)
            : this(context, new NoOpPdfStorageService(), NullLogger<PdfService>.Instance)
        {
        }

        public PdfService(ApplicationDbContext context, IPdfStorageService pdfStorageService, ILogger<PdfService> logger)
        {
            _context = context;
            _pdfStorageService = pdfStorageService;
            _logger = logger;
        }

        public async Task<byte[]> GenerateDossierPdfAsync(int clienteId, int? negozioId, bool isAdmin, CancellationToken ct = default)
        {
            var cliente = await _context.Clienti.FindAsync([clienteId], ct);
            if (cliente == null) return Array.Empty<byte>();

            if (!isAdmin && (!negozioId.HasValue || cliente.NegozioId != negozioId.Value))
            {
                throw new UnauthorizedAccessException("Tenant isolation violated.");
            }


            var misure = await _context.Misure
                .Where(m => m.ClienteId == clienteId)
                .OrderByDescending(m => m.DataCreazione)
                .ToListAsync(ct);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text($"DOSSIER SARTORIALE — {cliente.Nome} {cliente.Cognome}")
                        .FontSize(20).SemiBold().FontColor(Colors.Blue.Darken4);
                    
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Text($"Codice Cliente: {cliente.ClientCode}");
                        col.Item().Text($"Email: {cliente.Email}");
                        col.Item().Text($"Telefono: {cliente.Telefono ?? "N/A"}");
                        col.Item().Text($"Indirizzo: {cliente.Indirizzo ?? "N/A"} - {cliente.CodicePostale ?? ""} {cliente.Citta ?? ""} ({cliente.StatoProvincia ?? ""}) - {cliente.Paese ?? ""}");
                        
                        col.Item().PaddingTop(20).Text("Registro Misure Sartoriali").FontSize(16).SemiBold();
                        
                        if (misure.Any())
                        {
                            foreach (var misura in misure)
                            {
                                col.Item().PaddingTop(10).Text($"• {misura.DataCreazione:dd MMM yyyy} - {misura.TipoMisura.ToUpper()}");
                                col.Item().Text($"  Note: {misura.Note}");
                            }
                        }
                        else
                        {
                            col.Item().PaddingTop(10).Text("Nessuna misura registrata in archivio.").Italic();
                        }
                    });
                    
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Pagina ");
                        x.CurrentPageNumber();
                        x.Span(" di ");
                        x.TotalPages();
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();

            // On-demand generation is preserved; storage is best-effort for future retrieval.
            var storagePath = $"dossiers/cliente-{clienteId}/dossier-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            try
            {
                await _pdfStorageService.SaveAsync(storagePath, pdfBytes, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PDF storage save failed for cliente {ClienteId}", clienteId);
            }

            return pdfBytes;
        }

        private sealed class NoOpPdfStorageService : IPdfStorageService
        {
            public Task<string> SaveAsync(string relativePath, byte[] content, CancellationToken ct = default) =>
                Task.FromResult(relativePath);

            public Task<byte[]?> GetAsync(string relativePath, CancellationToken ct = default) =>
                Task.FromResult<byte[]?>(null);
        }
    }
}
