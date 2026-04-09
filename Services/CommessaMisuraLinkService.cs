using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public class CommessaMisuraLinkService : ICommessaMisuraLinkService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommessaMisuraLinkService> _logger;

        public CommessaMisuraLinkService(
            ApplicationDbContext context,
            ILogger<CommessaMisuraLinkService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result> LinkMisuraAsync(
            int id, int misuraClienteId, string? userId, int? negozioId, bool isAdmin)
        {
            var (commessa, authError) = await CommessaAccessHelper.FetchAndAuthorizeAsync(
                _context.Commissioni.Where(c => c.Id == id),
                c => c.NegozioId, negozioId, isAdmin,
                "Commessa non trovata.", "Accesso negato alla commessa.");

            if (commessa is null) return Result.Fail(authError!);

            var misura = await _context.Misure.FirstOrDefaultAsync(m => m.Id == misuraClienteId);
            if (misura == null || misura.ClienteId != commessa.ClienteId)
                return Result.Fail("Misura non valida per il cliente della commessa.");

            var exists = await _context.CommissioniMisureLinks
                .AnyAsync(x => x.CommessaSartorialeId == id && x.MisuraClienteId == misuraClienteId);

            if (exists)
                return await PromoteToMisureRaccolteIfNeededAsync(commessa, userId);

            return await ExecuteLinkMisuraAsync(id, misuraClienteId, misura, commessa, userId);
        }

        public async Task<bool> LinkDynamicMeasurementRecordAsync(
            int id, int dynamicRecordId, string? userId, int? negozioId, bool isAdmin)
        {
            var result = await LinkDynamicMeasurementRecordInternalAsync(id, dynamicRecordId, userId, negozioId, isAdmin);
            return result.IsSuccess;
        }

        public async Task<Result> UnlinkMisuraAsync(
            int id, int misuraClienteId, int? negozioId, bool isAdmin)
        {
            var (commessa, authError) = await CommessaAccessHelper.FetchAndAuthorizeAsync(
                _context.Commissioni.Where(c => c.Id == id),
                c => c.NegozioId, negozioId, isAdmin,
                "Commessa non trovata.", "Accesso negato alla commessa.");

            if (commessa is null) return Result.Fail(authError!);

            var link = await _context.CommissioniMisureLinks
                .FirstOrDefaultAsync(x => x.CommessaSartorialeId == id && x.MisuraClienteId == misuraClienteId);
            if (link == null)
                return Result.Fail("Collegamento misura non trovato.");

            var linkedCount = await _context.CommissioniMisureLinks
                .CountAsync(x => x.CommessaSartorialeId == id);

            var isRemovingLastLinkedMeasure = linkedCount <= 1;
            var shouldDemoteToBozza = commessa.Stato == StatoCommessa.MisureRaccolte && isRemovingLastLinkedMeasure;

            if (CommessaAccessHelper.RichiedeMisuraCollegata(commessa.Stato) && isRemovingLastLinkedMeasure && !shouldDemoteToBozza)
                return Result.Fail($"Nello stato '{commessa.Stato}' è obbligatorio mantenere almeno una misura collegata.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.CommissioniMisureLinks.Remove(link);

                _context.CommissioniEventi.Add(new CommessaEvento
                {
                    CommessaSartorialeId = id,
                    TipoEvento = "UnlinkMisura",
                    Descrizione = $"Scollegata misura id {misuraClienteId}.",
                    CreatedAt = DateTime.UtcNow
                });

                if (shouldDemoteToBozza)
                {
                    commessa.Stato = StatoCommessa.Bozza;
                    _context.CommissioniEventi.Add(new CommessaEvento
                    {
                        CommessaSartorialeId = id,
                        TipoEvento = "CambioStato",
                        NuovoStato = StatoCommessa.Bozza,
                        Descrizione = "Stato riportato a Bozza dopo la rimozione dell'ultima misura collegata.",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Result.Ok();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result.Fail("Errore durante lo scollegamento.");
            }
        }

        public async Task<Result> LinkDynamicMeasurementRecordInternalAsync(
            int id, int dynamicRecordId, string? userId, int? negozioId, bool isAdmin)
        {
            var misuraClienteId = await _context.Misure
                .Where(m => m.IsDynamic && m.RecordId == dynamicRecordId)
                .Select(m => (int?)m.Id)
                .FirstOrDefaultAsync();

            if (!misuraClienteId.HasValue)
                return Result.Fail("Misura dinamica non trovata nel registro.");

            return await LinkMisuraAsync(id, misuraClienteId.Value, userId, negozioId, isAdmin);
        }

        private async Task<Result> ExecuteLinkMisuraAsync(
            int id, int misuraClienteId, MisureCliente misura,
            CommessaSartoriale commessa, string? userId)
        {
            var ownsTransaction = _context.Database.CurrentTransaction == null;
            await using var transaction = ownsTransaction
                ? await _context.Database.BeginTransactionAsync()
                : null;

            try
            {
                _context.CommissioniMisureLinks.Add(new CommessaMisuraLink
                {
                    CommessaSartorialeId = id,
                    MisuraClienteId = misuraClienteId,
                    LinkedAt = DateTime.UtcNow,
                    LinkedByUserId = userId
                });

                _context.CommissioniEventi.Add(new CommessaEvento
                {
                    CommessaSartorialeId = id,
                    TipoEvento = "LinkMisura",
                    Descrizione = $"Collegata misura {misura.TipoMisura} del {misura.DataCreazione:dd/MM/yyyy HH:mm}.",
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                });

                var statusSync = await PromoteToMisureRaccolteIfNeededAsync(commessa, userId, deferSave: true);
                if (!statusSync.IsSuccess)
                {
                    if (ownsTransaction && transaction != null) await transaction.RollbackAsync();
                    return statusSync;
                }

                await _context.SaveChangesAsync();
                if (ownsTransaction && transaction != null) await transaction.CommitAsync();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                if (ownsTransaction && transaction != null) await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "Errore durante il collegamento della misura {MisuraClienteId} alla commessa {CommessaId}",
                    misuraClienteId, id);
                return Result.Fail("Errore durante il collegamento della misura.");
            }
        }

        private async Task<Result> PromoteToMisureRaccolteIfNeededAsync(
            CommessaSartoriale commessa, string? userId, bool deferSave = false)
        {
            if (commessa.Stato != StatoCommessa.Bozza)
                return Result.Ok();

            commessa.Stato = StatoCommessa.MisureRaccolte;
            _context.CommissioniEventi.Add(new CommessaEvento
            {
                CommessaSartorialeId = commessa.Id,
                TipoEvento = "CambioStato",
                NuovoStato = StatoCommessa.MisureRaccolte,
                Descrizione = "Stato aggiornato automaticamente a MisureRaccolte dopo il collegamento della prima misura.",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            if (!deferSave)
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Errore durante l'aggiornamento automatico dello stato della commessa {CommessaId}",
                        commessa.Id);
                    return Result.Fail("Errore durante l'aggiornamento automatico dello stato della commessa.");
                }
            }

            return Result.Ok();
        }
    }
}
