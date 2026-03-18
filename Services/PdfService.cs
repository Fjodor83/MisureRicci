using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MisureRicci.Services
{
    public class PdfService : IPdfService
    {
        private readonly ApplicationDbContext _context;

        public PdfService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateDossierPdfAsync(int clienteId)
        {
            var cliente = await _context.Clienti.FindAsync(clienteId);
            if (cliente == null) return Array.Empty<byte>();

            var misure = await _context.RegistroMisure
                .Where(m => m.ClienteId == clienteId)
                .OrderByDescending(m => m.DataCreazione)
                .ToListAsync();

            QuestPDF.Settings.License = LicenseType.Community;

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

            return document.GeneratePdf();
        }
    }
}
