using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public class CommessaService : ICommessaService
    {
        private const string ActiveMeasurementTypesCacheKey = "active_measurement_types_v1";
        private const string CommissioneNonTrovata = "Commissione non trovata.";
        private const string AccessoNegatoAllaCommissione = "Accesso negato alla commissione.";
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CommessaService> _logger;
        private readonly ICustomMeasurementService? _customMeasurementService;

        private static readonly Dictionary<StatoCommessa, StatoCommessa[]> AllowedTransitions = new()
        {
            [StatoCommessa.Bozza] = new[] { StatoCommessa.MisureRaccolte, StatoCommessa.Annullata },
            [StatoCommessa.MisureRaccolte] = new[] { StatoCommessa.InLavorazione, StatoCommessa.Annullata },
            [StatoCommessa.InLavorazione] = new[] { StatoCommessa.Prova1, StatoCommessa.ProntaConsegna, StatoCommessa.Annullata },
            [StatoCommessa.Prova1] = new[] { StatoCommessa.Prova2, StatoCommessa.InLavorazione, StatoCommessa.Annullata },
            [StatoCommessa.Prova2] = new[] { StatoCommessa.InLavorazione, StatoCommessa.ProntaConsegna, StatoCommessa.Annullata },
            [StatoCommessa.ProntaConsegna] = new[] { StatoCommessa.Consegnata, StatoCommessa.InLavorazione, StatoCommessa.Annullata },
            [StatoCommessa.Consegnata] = Array.Empty<StatoCommessa>(),
            [StatoCommessa.Annullata] = Array.Empty<StatoCommessa>()
        };

        // ── refactoring #2: HashSet O(1) invece di 6 confronti OR ──────────────
        private static readonly HashSet<StatoCommessa> StatiConMisuraObbligatoria = new()
        {
            StatoCommessa.MisureRaccolte,
            StatoCommessa.InLavorazione,
            StatoCommessa.Prova1,
            StatoCommessa.Prova2,
            StatoCommessa.ProntaConsegna,
            StatoCommessa.Consegnata
        };

        // ── refactoring #3: costruttore unico con parametro opzionale ───────────
        public CommessaService(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<CommessaService> logger,
            ICustomMeasurementService? customMeasurementService = null)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _customMeasurementService = customMeasurementService;
        }

        // ───────────────────────────────────────────────────────────────────────
        //  PUBLIC METHODS
        // ───────────────────────────────────────────────────────────────────────

        public async Task<PagedResult<CommessaSartoriale>> GetCommissioniPagedAsync(
            int? clienteId, int? negozioId, bool isAdmin, int page, int pageSize)
        {
            var query = _context.Commissioni
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(c => c.ClienteId == clienteId.Value);

            if (!isAdmin)
            {
                if (!negozioId.HasValue)
                    return new PagedResult<CommessaSartoriale>(Array.Empty<CommessaSartoriale>(), 0, page, pageSize);

                query = query.Where(c => c.NegozioId == negozioId.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.DataApertura)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CommessaSartoriale>(items, totalCount, page, pageSize);
        }

        public async Task<CommessaKpiViewModel> GetKpiAsync(int? negozioId, bool isAdmin)
        {
            var query = _context.Commissioni.AsNoTracking().AsQueryable();

            if (!isAdmin)
            {
                if (!negozioId.HasValue)
                    return new CommessaKpiViewModel();

                query = query.Where(c => c.NegozioId == negozioId.Value);
            }

            var now = DateTime.UtcNow;
            var snapshot = await query
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    Total = group.Count(),
                    Consegnate = group.Count(c => c.Stato == StatoCommessa.Consegnata),
                    Annullate = group.Count(c => c.Stato == StatoCommessa.Annullata),
                    InRitardo = group.Count(c =>
                        c.Stato != StatoCommessa.Consegnata &&
                        c.Stato != StatoCommessa.Annullata &&
                        c.DataConsegnaPrevista.HasValue &&
                        c.DataConsegnaPrevista.Value < now)
                })
                .FirstOrDefaultAsync();

            if (snapshot == null)
                return new CommessaKpiViewModel();

            return new CommessaKpiViewModel
            {
                Totale = snapshot.Total,
                Consegnate = snapshot.Consegnate,
                InCorso = snapshot.Total - snapshot.Consegnate - snapshot.Annullate,
                InRitardo = snapshot.InRitardo
            };
        }

        public async Task<CommessaSartoriale?> GetCommessaByIdAsync(int id, int? negozioId, bool isAdmin)
        {
            // ── refactoring #4: rimossa variabile intermedia inutile ────────────
            var commessa = await _context.Commissioni
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .Include(c => c.Eventi.OrderByDescending(e => e.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (commessa == null)
                return null;

            if (!CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin))
                return null;

            return commessa;
        }

        public async Task<CommessaDetailsViewModel?> GetCommessaDetailsAsync(int id, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.Commissioni
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .Include(c => c.Eventi.OrderByDescending(e => e.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (commessa == null)
                return null;

            if (!CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin))
                return null;

            var misureConStatoLink = await _context.Misure
                .AsNoTracking()
                .Where(m => m.ClienteId == commessa.ClienteId)
                .GroupJoin(
                    _context.CommissioniMisureLinks
                        .AsNoTracking()
                        .Where(l => l.CommessaSartorialeId == commessa.Id),
                    misura => misura.Id,
                    link => link.MisuraClienteId,
                    (misura, links) => new
                    {
                        Item = new CommessaMisuraItem
                        {
                            MisuraClienteId = misura.Id,
                            RecordId = misura.RecordId,
                            TipoMisura = misura.TipoMisura,
                            IsDynamic = misura.IsDynamic,
                            DataCreazione = misura.DataCreazione,
                            Note = misura.Note ?? misura.SystemNote
                        },
                        IsLinked = links.Any()
                    })
                .OrderByDescending(x => x.Item.DataCreazione)
                .ToListAsync();

            var linked = misureConStatoLink
                .Where(x => x.IsLinked)
                .Select(x =>
                {
                    x.Item.IsRecommended = IsMeasurementRecommendedForTipoCapo(commessa.TipoCapo, x.Item.TipoMisura);
                    return x.Item;
                })
                .ToList();

            var free = misureConStatoLink
                .Where(x => !x.IsLinked)
                .Select(x =>
                {
                    x.Item.IsRecommended = IsMeasurementRecommendedForTipoCapo(commessa.TipoCapo, x.Item.TipoMisura);
                    return x.Item;
                })
                .ToList();

            var totalMisureCliente = linked.Count + free.Count;

            var misuraStatus = new CommessaMisuraStatus
            {
                HasMisureCollegate = linked.Count > 0,
                HasMisureDisponibili = free.Count > 0,
                RequireMisuraCreation = totalMisureCliente == 0,
                TotaleMisureCliente = totalMisureCliente
            };

            var measurementTypes = await GetActiveMeasurementTypesAsync();

            return new CommessaDetailsViewModel
            {
                Commessa = commessa,
                StatiDisponibili = GetAllowedNextStates(commessa.Stato),
                MisureCollegate = linked,
                MisureDisponibili = free,
                HasLinkedMeasureTypeMismatch = linked.Count > 0 && !linked.Any(x => x.IsRecommended),
                MisuraStatus = misuraStatus,
                MeasurementTypes = measurementTypes
            };
        }

        public async Task<List<CommessaMisuraItem>> GetMisureDisponibiliPerClienteAsync(
            int clienteId, int? negozioId, bool isAdmin)
        {
            if (!isAdmin)
            {
                if (!negozioId.HasValue)
                    return new List<CommessaMisuraItem>();

                var hasClienteAccess = await _context.Clienti
                    .AsNoTracking()
                    .AnyAsync(c => c.Id == clienteId && c.NegozioId == negozioId.Value);

                if (!hasClienteAccess)
                    return new List<CommessaMisuraItem>();
            }

            return await _context.Misure
                .AsNoTracking()
                .Where(m => m.ClienteId == clienteId)
                .OrderByDescending(m => m.DataCreazione)
                .Select(m => new CommessaMisuraItem
                {
                    MisuraClienteId = m.Id,
                    RecordId = m.RecordId,
                    TipoMisura = m.TipoMisura,
                    IsDynamic = m.IsDynamic,
                    DataCreazione = m.DataCreazione,
                    Note = m.Note ?? m.SystemNote
                })
                .ToListAsync();
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

            if (!CanAccessNegozio(cliente.NegozioId, negozioId, isAdmin))
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

        public async Task<Result<int>> CreateAndLinkDynamicMeasurementAsync(
            DynamicMeasurementCreateViewModel model, string? userId, int? negozioId, bool isAdmin)
        {
            if (!model.ReturnToCommessaId.HasValue)
                return Result<int>.Fail("Commessa di destinazione non specificata.");

            if (_customMeasurementService == null)
                return Result<int>.Fail("Servizio misure dinamiche non disponibile.");

            // ── refactoring #1: FetchAndAuthorizeAsync ──────────────────────────
            var (commessa, authError) = await FetchAndAuthorizeAsync(
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
                var linkResult = await LinkDynamicMeasurementRecordInternalAsync(
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
            // ── refactoring #1: FetchAndAuthorizeAsync ──────────────────────────
            var (commessa, authError) = await FetchAndAuthorizeAsync(
                _context.Commissioni.Where(c => c.Id == id),
                c => c.NegozioId, negozioId, isAdmin,
                CommissioneNonTrovata, AccessoNegatoAllaCommissione);

            if (commessa is null) return Result.Fail(authError!);

            if (commessa.Stato == nuovoStato)
                return Result.Ok();

            var allowed = GetAllowedNextStates(commessa.Stato);
            if (!allowed.Contains(nuovoStato))
                return Result.Fail($"Transizione da {commessa.Stato} a {nuovoStato} non consentita.");

            if (RichiedeMisuraCollegata(nuovoStato))
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

            // ── refactoring #1: FetchAndAuthorizeAsync ──────────────────────────
            var (commessa, authError) = await FetchAndAuthorizeAsync(
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

        public async Task<Result> LinkMisuraAsync(
            int id, int misuraClienteId, string? userId, int? negozioId, bool isAdmin)
        {
            // ── refactoring #1: FetchAndAuthorizeAsync ──────────────────────────
            var (commessa, authError) = await FetchAndAuthorizeAsync(
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

            // ── refactoring #5: logica transazionale delegata a metodo dedicato ─
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
            // ── refactoring #1: FetchAndAuthorizeAsync ──────────────────────────
            var (commessa, authError) = await FetchAndAuthorizeAsync(
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

            if (RichiedeMisuraCollegata(commessa.Stato) && isRemovingLastLinkedMeasure && !shouldDemoteToBozza)
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

        public async Task<CommessaMisuraStatus> GetStatoMisureClienteAsync(
            int commessaId, int? negozioId, bool isAdmin)
        {
            // ── refactoring #1: FetchAndAuthorizeAsync ──────────────────────────
            var (commessa, _) = await FetchAndAuthorizeAsync(
                _context.Commissioni
                    .AsNoTracking()
                    .Select(c => new { c.Id, c.ClienteId, c.NegozioId })
                    .Where(c => c.Id == commessaId),
                c => c.NegozioId, negozioId, isAdmin);

            if (commessa is null)
                return new CommessaMisuraStatus();

            var totaleMisureCliente = await _context.Misure
                .CountAsync(m => m.ClienteId == commessa.ClienteId);

            var linkedCount = await _context.CommissioniMisureLinks
                .CountAsync(x => x.CommessaSartorialeId == commessaId);

            return new CommessaMisuraStatus
            {
                HasMisureCollegate = linkedCount > 0,
                HasMisureDisponibili = totaleMisureCliente > linkedCount,
                RequireMisuraCreation = totaleMisureCliente == 0,
                TotaleMisureCliente = totaleMisureCliente
            };
        }

        // ───────────────────────────────────────────────────────────────────────
        //  PRIVATE HELPERS
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Recupera la prima entità dalla query e verifica l'accesso al negozio.
        /// Restituisce (entity, null) in caso di successo, (null, errorMessage) altrimenti.
        /// </summary>
        private static async Task<(T? Value, string? Error)> FetchAndAuthorizeAsync<T>(
            IQueryable<T> query,
            Func<T, int?> negozioIdSelector,
            int? userNegozioId,
            bool isAdmin,
            string notFoundMessage = "Risorsa non trovata.",
            string forbidMessage = "Accesso negato.")
            where T : class
        {
            var entity = await query.FirstOrDefaultAsync();
            if (entity is null)
                return (null, notFoundMessage);
            if (!CanAccessNegozio(negozioIdSelector(entity), userNegozioId, isAdmin))
                return (null, forbidMessage);
            return (entity, null);
        }

        /// <summary>
        /// Esegue il collegamento della misura in un'unica unità transazionale,
        /// gestendo sia transazioni proprie che transazioni già aperte dal chiamante.
        /// </summary>
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

        private static List<StatoCommessa> GetAllowedNextStates(StatoCommessa current)
        {
            if (!AllowedTransitions.TryGetValue(current, out var items))
                return new List<StatoCommessa>();

            return items.ToList();
        }

        // ── refactoring #2: HashSet O(1) ────────────────────────────────────────
        private static bool RichiedeMisuraCollegata(StatoCommessa stato) =>
            StatiConMisuraObbligatoria.Contains(stato);

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

        private async Task<Result> LinkDynamicMeasurementRecordInternalAsync(
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

        private static bool IsMeasurementRecommendedForTipoCapo(string tipoCapo, string tipoMisura)
        {
            if (string.IsNullOrWhiteSpace(tipoCapo) || string.IsNullOrWhiteSpace(tipoMisura))
                return false;

            var firstTipoCapoWord = tipoCapo
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            return tipoCapo.Contains(tipoMisura, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(firstTipoCapoWord)
                    && tipoMisura.Contains(firstTipoCapoWord, StringComparison.OrdinalIgnoreCase));
        }

        private static bool CanAccessNegozio(int? commessaNegozioId, int? userNegozioId, bool isAdmin)
        {
            if (isAdmin) return true;

            return userNegozioId.HasValue
                && commessaNegozioId.HasValue
                && commessaNegozioId.Value == userNegozioId.Value;
        }

        private async Task<List<MeasurementType>> GetActiveMeasurementTypesAsync()
        {
            return await _cache.GetOrCreateAsync(ActiveMeasurementTypesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _context.DynamicMeasurementTypes
                    .AsNoTracking()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Nome)
                    .ToListAsync();
            }) ?? new List<MeasurementType>();
        }
    }
}