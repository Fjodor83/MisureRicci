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
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CommessaService> _logger;

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

        public CommessaService(ApplicationDbContext context, IMemoryCache cache, ILogger<CommessaService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PagedResult<CommessaSartoriale>> GetCommissioniPagedAsync(int? clienteId, int? negozioId, bool isAdmin, int page, int pageSize)
        {
            var query = _context.Commissioni
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .AsQueryable();

            if (clienteId.HasValue)
            {
                query = query.Where(c => c.ClienteId == clienteId.Value);
            }

            if (!isAdmin)
            {
                if (!negozioId.HasValue)
                {
                    return new PagedResult<CommessaSartoriale>(Array.Empty<CommessaSartoriale>(), 0, page, pageSize);
                }

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
                {
                    return new CommessaKpiViewModel();
                }

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
            {
                return new CommessaKpiViewModel();
            }

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
            var commessaTask = _context.Commissioni
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .Include(c => c.Eventi.OrderByDescending(e => e.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == id);

            var commessa = await commessaTask;
            if (commessa == null)
            {
                return null;
            }

            if (!CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin))
            {
                return null;
            }

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
            {
                return null;
            }

            if (!CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin))
            {
                return null;
            }

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
                .Select(x => x.Item)
                .ToList();

            var free = misureConStatoLink
                .Where(x => !x.IsLinked)
                .Select(x => x.Item)
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
                MisuraStatus = misuraStatus,
                MeasurementTypes = measurementTypes
            };
        }

        public async Task<List<CommessaMisuraItem>> GetMisureDisponibiliPerClienteAsync(int clienteId, int? negozioId, bool isAdmin)
        {
            if (!isAdmin)
            {
                if (!negozioId.HasValue)
                {
                    return new List<CommessaMisuraItem>();
                }

                var hasClienteAccess = await _context.Clienti
                    .AsNoTracking()
                    .AnyAsync(c => c.Id == clienteId && c.NegozioId == negozioId.Value);

                if (!hasClienteAccess)
                {
                    return new List<CommessaMisuraItem>();
                }
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

        public async Task<Result<CommessaSartoriale>> CreateCommessaAsync(CommessaCreateViewModel model, string? userId, int? negozioId, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(model.TipoCapo))
            {
                return Result<CommessaSartoriale>.Fail("Il tipo capo è obbligatorio.");
            }

            var cliente = await _context.Clienti
                .AsNoTracking()
                .Select(c => new { c.Id, c.NegozioId })
                .FirstOrDefaultAsync(c => c.Id == model.ClienteId);
            if (cliente == null)
            {
                return Result<CommessaSartoriale>.Fail("Cliente non trovato.");
            }

            if (!CanAccessNegozio(cliente.NegozioId, negozioId, isAdmin))
            {
                return Result<CommessaSartoriale>.Fail("Accesso negato al cliente.");
            }

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
                {
                    return Result<CommessaSartoriale>.Fail("Una o più misure selezionate non sono valide per il cliente.");
                }

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
                Stato = StatoCommessa.Bozza,
                DataApertura = now,
                CreatedByUserId = userId
            };

            _context.Commissioni.Add(entity);
            await _context.SaveChangesAsync();

            if (string.IsNullOrWhiteSpace(entity.CommessaCode))
            {
                entity.CommessaCode = $"CM-{now.Year}-{entity.Id:D6}";
            }

            _context.CommissioniEventi.Add(new CommessaEvento
            {
                CommessaSartorialeId = entity.Id,
                TipoEvento = "Apertura",
                NuovoStato = entity.Stato,
                Descrizione = "Commessa aperta.",
                CreatedByUserId = userId,
                CreatedAt = now
            });

            // Link pre-selected measurements
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

        public async Task<Result> AdvanceStatoAsync(int id, StatoCommessa nuovoStato, string? note, string? userId, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.Commissioni.FirstOrDefaultAsync(c => c.Id == id);
            if (commessa == null)
            {
                return Result.Fail("Commessa non trovata.");
            }

            if (!CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin))
            {
                return Result.Fail("Accesso negato alla commessa.");
            }

            if (commessa.Stato == nuovoStato)
            {
                return Result.Ok();
            }

            var allowed = GetAllowedNextStates(commessa.Stato);
            if (!allowed.Contains(nuovoStato))
            {
                return Result.Fail($"Transizione da {commessa.Stato} a {nuovoStato} non consentita.");
            }

            // Operational safeguard: at least one linked measurement is required for production states.
            if (RichiedeMisuraCollegata(nuovoStato))
            {
                var hasLinkedMeasure = await _context.CommissioniMisureLinks
                    .AnyAsync(x => x.CommessaSartorialeId == commessa.Id);
                if (!hasLinkedMeasure)
                {
                    return Result.Fail("Almeno una misura collegata è richiesta per avanzare in produzione.");
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                commessa.Stato = nuovoStato;
                if (nuovoStato == StatoCommessa.Consegnata && commessa.DataConsegnaEffettiva == null)
                {
                    commessa.DataConsegnaEffettiva = DateTime.UtcNow;
                }

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

        public async Task<Result> AddNotaAsync(int id, string nota, string? userId, int? negozioId, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(nota))
            {
                return Result.Fail("La nota non può essere vuota.");
            }

            var commessa = await _context.Commissioni.FirstOrDefaultAsync(c => c.Id == id);
            if (commessa == null)
            {
                return Result.Fail("Commessa non trovata.");
            }

            if (!CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin))
            {
                return Result.Fail("Accesso negato alla commessa.");
            }

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

        public async Task<Result> LinkMisuraAsync(int id, int misuraClienteId, string? userId, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.Commissioni.FirstOrDefaultAsync(c => c.Id == id);
            if (commessa == null)
            {
                return Result.Fail("Commessa non trovata.");
            }

            if (!CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin))
            {
                return Result.Fail("Accesso negato alla commessa.");
            }

            var misura = await _context.Misure.FirstOrDefaultAsync(m => m.Id == misuraClienteId);
            if (misura == null || misura.ClienteId != commessa.ClienteId)
            {
                return Result.Fail("Misura non valida per il cliente della commessa.");
            }

            var exists = await _context.CommissioniMisureLinks
                .AnyAsync(x => x.CommessaSartorialeId == id && x.MisuraClienteId == misuraClienteId);
            if (exists)
            {
                return Result.Ok();
            }

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

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<bool> LinkDynamicMeasurementRecordAsync(int id, int dynamicRecordId, string? userId, int? negozioId, bool isAdmin)
        {
            var misuraClienteId = await _context.Misure
                .Where(m => m.IsDynamic && m.RecordId == dynamicRecordId)
                .Select(m => (int?)m.Id)
                .FirstOrDefaultAsync();

            if (!misuraClienteId.HasValue)
            {
                return false;
            }

            var result = await LinkMisuraAsync(id, misuraClienteId.Value, userId, negozioId, isAdmin);
            return result.IsSuccess;
        }

        public async Task<Result> UnlinkMisuraAsync(int id, int misuraClienteId, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.Commissioni.FirstOrDefaultAsync(c => c.Id == id);
            if (commessa == null)
            {
                return Result.Fail("Commessa non trovata.");
            }

            if (!CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin))
            {
                return Result.Fail("Accesso negato alla commessa.");
            }

            // Safeguard: cannot unlink if state requires at least one measure and this is the last one.
            if (RichiedeMisuraCollegata(commessa.Stato))
            {
                var count = await _context.CommissioniMisureLinks
                    .CountAsync(x => x.CommessaSartorialeId == id);
                if (count <= 1)
                {
                    return Result.Fail($"Nello stato '{commessa.Stato}' è obbligatorio mantenere almeno una misura collegata.");
                }
            }

            var link = await _context.CommissioniMisureLinks
                .FirstOrDefaultAsync(x => x.CommessaSartorialeId == id && x.MisuraClienteId == misuraClienteId);
            if (link == null)
            {
                return Result.Fail("Collegamento misura non trovato.");
            }

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

        private static List<StatoCommessa> GetAllowedNextStates(StatoCommessa current)
        {
            if (!AllowedTransitions.TryGetValue(current, out var items))
            {
                return new List<StatoCommessa>();
            }

            return items.ToList();
        }

        private static bool RichiedeMisuraCollegata(StatoCommessa stato)
        {
            return stato == StatoCommessa.InLavorazione
                || stato == StatoCommessa.Prova1
                || stato == StatoCommessa.Prova2
                || stato == StatoCommessa.ProntaConsegna
                || stato == StatoCommessa.Consegnata;
        }

        private static bool CanAccessNegozio(int? commessaNegozioId, int? userNegozioId, bool isAdmin)
        {
            if (isAdmin)
            {
                return true;
            }

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

        public async Task<CommessaMisuraStatus> GetStatoMisureClienteAsync(int commessaId, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.Commissioni
                .AsNoTracking()
                .Select(c => new { c.Id, c.ClienteId, c.NegozioId })
                .FirstOrDefaultAsync(c => c.Id == commessaId);

            if (commessa == null)
            {
                return new CommessaMisuraStatus();
            }

            if (!CanAccessNegozio(commessa.NegozioId, negozioId, isAdmin))
            {
                return new CommessaMisuraStatus();
            }

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
    }
}
