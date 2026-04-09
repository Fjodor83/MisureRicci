using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public class CommessaCommandService : ICommessaCommandService
    {
        private const string CommissioneNonTrovata = "Commissione non trovata.";
        private const string AccessoNegatoAllaCommissione = "Accesso negato alla commissione.";
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommessaCommandService> _logger;
        private readonly ICustomMeasurementService? _customMeasurementService;
        private readonly ICommessaMisuraLinkService _linkService;

        public CommessaCommandService(
            ApplicationDbContext context,
            ILogger<CommessaCommandService> logger,
            ICommessaMisuraLinkService linkService,
            ICustomMeasurementService? customMeasurementService = null)
        {
            _context = context;
            _logger = logger;
            _linkService = linkService;
            _customMeasurementService = customMeasurementService;
        }

        public async Task<Result<CommessaSartoriale>> CreateCommessaAsync(
            CommessaCreateViewModel model, string? userId, int? negozioId, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(model.TipoCapo))
                return Result<CommessaSartoriale>.Fail("Il tipo capo è obbligatorio.");

            var cliente = await _context.Clienti
                .AsNoTracking()
                .Select(c => new { c.Id, c.NegozioId })
                .FirstOrDefaultAsync(c => c.Id == model.ClienteId);

            if (cliente == null)
                return Result<CommessaSartoriale>.Fail("Cliente non trovato.");

            if (!CommessaAccessHelper.CanAccessNegozio(cliente.NegozioId, negozioId, isAdmin))
                return Result<CommessaSartoriale>.Fail("Accesso negato al cliente.");

            var selectedMisuraIds = model.SelectedMisuraIds
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            var selectedMisure = new Dictionary<int, MisureCliente>();
            if (selectedMisuraIds.Length > 0)
            {
                var misure = await _context.Misure
                    .AsNoTracking()
                    .Where(m => selectedMisuraIds.Contains(m.Id) && m.ClienteId == model.ClienteId)
                    .ToListAsync();

                if (misure.Count != selectedMisuraIds.Length)
                    return Result<CommessaSartoriale>.Fail("Una o più misure selezionate non sono valide per il cliente.");

                selectedMisure = misure.ToDictionary(m => m.Id);
            }

            var now = DateTime.UtcNow;
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var entity = new CommessaSartoriale
            {
                ClienteId = model.ClienteId,
                NegozioId = cliente.NegozioId,
                TipoCapo = model.TipoCapo.Trim(),
                Tessuto = string.IsNullOrWhiteSpace(model.Tessuto) ? null : model.Tessuto.Trim(),
                Collezione = string.IsNullOrWhiteSpace(model.Collezione) ? null : model.Collezione.Trim(),
                DataConsegnaPrevista = model.DataConsegnaPrevista,
                NoteInterne = string.IsNullOrWhiteSpace(model.NoteInterne) ? null : model.NoteInterne.Trim(),
                Stato = selectedMisuraIds.Length > 0 ? StatoCommessa.MisureRaccolte : StatoCommessa.Bozza,
                DataApertura = now,
                CreatedByUserId = userId
            };

            _context.Commissioni.Add(entity);
            await _context.SaveChangesAsync();

            if (string.IsNullOrWhiteSpace(entity.CommessaCode))
                entity.CommessaCode = $"CM-{now.Year}-{entity.Id:D6}";

            _context.CommissioniEventi.Add(new CommessaEvento
            {
                CommessaSartorialeId = entity.Id,
                TipoEvento = "Apertura",
                NuovoStato = entity.Stato,
                Descrizione = "Commessa aperta.",
                CreatedByUserId = userId,
                CreatedAt = now
            });

            if (selectedMisuraIds.Length > 0)
            {
                foreach (var misuraId in selectedMisuraIds)
                {
                    var misura = selectedMisure[misuraId];

                    _context.CommissioniMisureLinks.Add(new CommessaMisuraLink
                    {
                        CommessaSartorialeId = entity.Id,
                        MisuraClienteId = misuraId,
                        LinkedAt = now,
                        LinkedByUserId = userId
                    });

                    _context.CommissioniEventi.Add(new CommessaEvento
                    {
                        CommessaSartorialeId = entity.Id,
                        TipoEvento = "LinkMisura",
                        Descrizione = $"Collegata misura {misura.TipoMisura} (scelta in creazione) del {misura.DataCreazione:dd/MM/yyyy HH:mm}.",
                        CreatedByUserId = userId,
                        CreatedAt = now
                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Result<CommessaSartoriale>.Ok(entity);
        }

        public async Task<Result> DeleteCommessaAsync(int id, int? negozioId, bool isAdmin)
        {
            var (commessa, authError) = await CommessaAccessHelper.FetchAndAuthorizeAsync(
                _context.Commissioni.Where(c => c.Id == id),
                c => c.NegozioId, negozioId, isAdmin,
                CommissioneNonTrovata, AccessoNegatoAllaCommissione);

            if (commessa is null)
                return Result.Fail(authError!);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var links = await _context.CommissioniMisureLinks
                    .Where(x => x.CommessaSartorialeId == id)
                    .ToListAsync();
                var eventi = await _context.CommissioniEventi
                    .Where(x => x.CommessaSartorialeId == id)
                    .ToListAsync();

                if (links.Count > 0)
                    _context.CommissioniMisureLinks.RemoveRange(links);

                if (eventi.Count > 0)
                    _context.CommissioniEventi.RemoveRange(eventi);

                _context.Commissioni.Remove(commessa);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Errore durante l'eliminazione della commessa {CommessaId}", id);
                return Result.Fail("Errore durante l'eliminazione della commessa.");
            }
        }

        public async Task<Result<int>> CreateAndLinkDynamicMeasurementAsync(
            DynamicMeasurementCreateViewModel model, string? userId, int? negozioId, bool isAdmin)
        {
            if (!model.ReturnToCommessaId.HasValue)
                return Result<int>.Fail("Commessa di destinazione non specificata.");

            if (_customMeasurementService == null)
                return Result<int>.Fail("Servizio misure dinamiche non disponibile.");

            var (commessa, authError) = await CommessaAccessHelper.FetchAndAuthorizeAsync(
                _context.Commissioni
                    .AsNoTracking()
                    .Select(c => new { c.Id, c.ClienteId, c.NegozioId })
                    .Where(c => c.Id == model.ReturnToCommessaId.Value),
                c => c.NegozioId, negozioId, isAdmin,
                CommissioneNonTrovata, AccessoNegatoAllaCommissione);

            if (commessa is null) return Result<int>.Fail(authError!);

            if (commessa.ClienteId != model.ClienteId)
                return Result<int>.Fail("La misura deve essere creata per lo stesso cliente della commessa.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var record = await _customMeasurementService.CreateDynamicMeasurementAsync(model, userId);
                var linkResult = await _linkService.LinkDynamicMeasurementRecordInternalAsync(
                    commessa.Id, record.Id, userId, negozioId, isAdmin);

                if (!linkResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Fail(linkResult.Error ?? "Impossibile collegare la misura alla commessa.");
                }

                await transaction.CommitAsync();
                return Result<int>.Ok(record.Id);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                return Result<int>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "Errore durante la creazione e collegamento della misura dinamica alla commessa {CommessaId}",
                    model.ReturnToCommessaId.Value);
                return Result<int>.Fail("Errore durante la creazione e il collegamento della misura.");
            }
        }

        public async Task<Result> AdvanceStatoAsync(
            int id, StatoCommessa nuovoStato, string? note, string? userId, int? negozioId, bool isAdmin)
        {
            var (commessa, authError) = await CommessaAccessHelper.FetchAndAuthorizeAsync(
                _context.Commissioni.Where(c => c.Id == id),
                c => c.NegozioId, negozioId, isAdmin,
                CommissioneNonTrovata, AccessoNegatoAllaCommissione);

            if (commessa is null) return Result.Fail(authError!);

            if (commessa.Stato == nuovoStato)
                return Result.Ok();

            var allowed = CommessaAccessHelper.GetAllowedNextStates(commessa.Stato);
            if (!allowed.Contains(nuovoStato))
                return Result.Fail($"Transizione da {commessa.Stato} a {nuovoStato} non consentita.");

            if (CommessaAccessHelper.RichiedeMisuraCollegata(nuovoStato))
            {
                var hasLinkedMeasure = await _context.CommissioniMisureLinks
                    .AnyAsync(x => x.CommessaSartorialeId == commessa.Id);
                if (!hasLinkedMeasure)
                    return Result.Fail("Almeno una misura collegata è richiesta per avanzare in produzione.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                commessa.Stato = nuovoStato;
                if (nuovoStato == StatoCommessa.Consegnata && commessa.DataConsegnaEffettiva == null)
                    commessa.DataConsegnaEffettiva = DateTime.UtcNow;

                _context.CommissioniEventi.Add(new CommessaEvento
                {
                    CommessaSartorialeId = commessa.Id,
                    TipoEvento = "CambioStato",
                    NuovoStato = nuovoStato,
                    Descrizione = string.IsNullOrWhiteSpace(note)
                        ? $"Stato aggiornato a {nuovoStato}."
                        : $"Stato aggiornato a {nuovoStato}. Nota: {note.Trim()}",
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Errore durante avanzamento stato commessa {Id}", id);
                return Result.Fail("Errore durante l'aggiornamento dello stato.");
            }
        }

        public async Task<Result> AddNotaAsync(
            int id, string nota, string? userId, int? negozioId, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(nota))
                return Result.Fail("La nota non può essere vuota.");

            var (commessa, authError) = await CommessaAccessHelper.FetchAndAuthorizeAsync(
                _context.Commissioni.Where(c => c.Id == id),
                c => c.NegozioId, negozioId, isAdmin,
                "Commessa non trovata.", "Accesso negato alla commessa.");

            if (commessa is null) return Result.Fail(authError!);

            var notaPulita = nota.Trim();
            var timestamp = $"[{DateTime.UtcNow.ToLocalTime():yyyy-MM-dd HH:mm}]";

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                commessa.NoteInterne = string.IsNullOrWhiteSpace(commessa.NoteInterne)
                    ? $"{timestamp} {notaPulita}"
                    : $"{commessa.NoteInterne}\n{timestamp} {notaPulita}";

                _context.CommissioniEventi.Add(new CommessaEvento
                {
                    CommessaSartorialeId = commessa.Id,
                    TipoEvento = "Nota",
                    NuovoStato = null,
                    Descrizione = notaPulita,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Result.Ok();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result.Fail("Errore durante il salvataggio della nota.");
            }
        }
    }
}
