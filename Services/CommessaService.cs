using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public class CommessaService : ICommessaService
    {
        private readonly ApplicationDbContext _context;

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

        public CommessaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<CommessaSartoriale> Items, int TotalCount)> GetCommissioniPagedAsync(int? clienteId, int? negozioId, bool isAdmin, int page, int pageSize)
        {
            var query = _context.CommissioniSartoriali
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .AsQueryable();

            if (clienteId.HasValue)
            {
                query = query.Where(c => c.ClienteId == clienteId.Value);
            }

            if (!isAdmin && negozioId.HasValue)
            {
                query = query.Where(c => c.NegozioId == negozioId.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.DataApertura)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<CommessaKpiViewModel> GetKpiAsync(int? negozioId, bool isAdmin)
        {
            var query = _context.CommissioniSartoriali.AsNoTracking().AsQueryable();

            if (!isAdmin && negozioId.HasValue)
            {
                query = query.Where(c => c.NegozioId == negozioId.Value);
            }

            var total = await query.CountAsync();
            var consegnate = await query.CountAsync(c => c.Stato == StatoCommessa.Consegnata);
            var annullate = await query.CountAsync(c => c.Stato == StatoCommessa.Annullata);
            var inRitardo = await query.CountAsync(c =>
                c.Stato != StatoCommessa.Consegnata &&
                c.Stato != StatoCommessa.Annullata &&
                c.DataConsegnaPrevista.HasValue &&
                c.DataConsegnaPrevista.Value < DateTime.UtcNow);

            return new CommessaKpiViewModel
            {
                Totale = total,
                Consegnate = consegnate,
                InCorso = total - consegnate - annullate,
                InRitardo = inRitardo
            };
        }

        public async Task<CommessaSartoriale?> GetCommessaByIdAsync(int id, int? negozioId, bool isAdmin)
        {
            var query = _context.CommissioniSartoriali
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .Include(c => c.Eventi.OrderByDescending(e => e.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == id);

            var commessa = await query;
            if (commessa == null)
            {
                return null;
            }

            if (!isAdmin && negozioId.HasValue && commessa.NegozioId != negozioId.Value)
            {
                return null;
            }

            return commessa;
        }

        public async Task<CommessaDetailsViewModel?> GetCommessaDetailsAsync(int id, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.CommissioniSartoriali
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Negozio)
                .Include(c => c.Eventi.OrderByDescending(e => e.CreatedAt))
                .Include(c => c.MisureCollegate)
                    .ThenInclude(l => l.MisuraCliente)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (commessa == null)
            {
                return null;
            }

            if (!isAdmin && negozioId.HasValue && commessa.NegozioId != negozioId.Value)
            {
                return null;
            }

            var linkedIds = commessa.MisureCollegate.Select(x => x.MisuraClienteId).ToHashSet();
            var disponibili = await _context.RegistroMisure
                .Where(m => m.ClienteId == commessa.ClienteId)
                .OrderByDescending(m => m.DataCreazione)
                .Select(m => new CommessaMisuraItem
                {
                    MisuraClienteId = m.Id,
                    RecordId = m.RecordId,
                    TipoMisura = m.TipoMisura,
                    IsDynamic = m.IsDynamic,
                    DataCreazione = m.DataCreazione,
                    Note = m.Note
                })
                .ToListAsync();

            var linked = disponibili.Where(x => linkedIds.Contains(x.MisuraClienteId)).ToList();
            var free = disponibili.Where(x => !linkedIds.Contains(x.MisuraClienteId)).ToList();

            var misuraStatus = new CommessaMisuraStatus
            {
                HasMisureCollegate = linked.Count > 0,
                HasMisureDisponibili = free.Count > 0,
                RequireMisuraCreation = disponibili.Count == 0,
                TotaleMisureCliente = disponibili.Count
            };

            var measurementTypes = await _context.MeasurementTypes
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Nome)
                .ToListAsync();

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

        public async Task<List<CommessaMisuraItem>> GetMisureDisponibiliPerClienteAsync(int clienteId)
        {
            return await _context.RegistroMisure
                .Where(m => m.ClienteId == clienteId)
                .OrderByDescending(m => m.DataCreazione)
                .Select(m => new CommessaMisuraItem
                {
                    MisuraClienteId = m.Id,
                    RecordId = m.RecordId,
                    TipoMisura = m.TipoMisura,
                    IsDynamic = m.IsDynamic,
                    DataCreazione = m.DataCreazione,
                    Note = m.Note
                })
                .ToListAsync();
        }

        public async Task<CommessaSartoriale> CreateCommessaAsync(CommessaCreateViewModel model, string? userId)
        {
            var cliente = await _context.Clienti.FirstOrDefaultAsync(c => c.Id == model.ClienteId)
                ?? throw new InvalidOperationException("Cliente non trovato.");

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
                DataApertura = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            _context.CommissioniSartoriali.Add(entity);
            await _context.SaveChangesAsync();

            if (string.IsNullOrWhiteSpace(entity.CommessaCode))
            {
                entity.CommessaCode = $"CM-{DateTime.UtcNow.Year}-{entity.Id:D6}";
            }

            _context.CommissioniEventi.Add(new CommessaEvento
            {
                CommessaSartorialeId = entity.Id,
                TipoEvento = "Apertura",
                NuovoStato = entity.Stato,
                Descrizione = "Commessa aperta.",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            // Link pre-selected measurements
            if (model.SelectedMisuraIds != null && model.SelectedMisuraIds.Any())
            {
                foreach (var misuraId in model.SelectedMisuraIds)
                {
                    var misura = await _context.RegistroMisure.FindAsync(misuraId);
                    if (misura != null && misura.ClienteId == entity.ClienteId)
                    {
                        _context.CommissioniMisureLinks.Add(new CommessaMisuraLink
                        {
                            CommessaSartorialeId = entity.Id,
                            MisuraClienteId = misuraId,
                            LinkedAt = DateTime.UtcNow,
                            LinkedByUserId = userId
                        });

                        _context.CommissioniEventi.Add(new CommessaEvento
                        {
                            CommessaSartorialeId = entity.Id,
                            TipoEvento = "LinkMisura",
                            Descrizione = $"Collegata misura {misura.TipoMisura} (scelta in creazione) del {misura.DataCreazione:dd/MM/yyyy HH:mm}.",
                            CreatedByUserId = userId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return entity;
        }

        public async Task<bool> AdvanceStatoAsync(int id, StatoCommessa nuovoStato, string? note, string? userId, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.CommissioniSartoriali.FirstOrDefaultAsync(c => c.Id == id);
            if (commessa == null)
            {
                return false;
            }

            if (!isAdmin && negozioId.HasValue && commessa.NegozioId != negozioId.Value)
            {
                return false;
            }

            if (commessa.Stato == nuovoStato)
            {
                return true;
            }

            var allowed = GetAllowedNextStates(commessa.Stato);
            if (!allowed.Contains(nuovoStato))
            {
                return false;
            }

            // Operational safeguard: at least one linked measurement is required after data collection.
            if (RichiedeMisuraCollegata(nuovoStato))
            {
                var hasLinkedMeasure = await _context.CommissioniMisureLinks
                    .AnyAsync(x => x.CommessaSartorialeId == commessa.Id);
                if (!hasLinkedMeasure)
                {
                    return false;
                }
            }

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
            return true;
        }

        public async Task<bool> AddNotaAsync(int id, string nota, string? userId, int? negozioId, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(nota))
            {
                return false;
            }

            var commessa = await _context.CommissioniSartoriali.FirstOrDefaultAsync(c => c.Id == id);
            if (commessa == null)
            {
                return false;
            }

            if (!isAdmin && negozioId.HasValue && commessa.NegozioId != negozioId.Value)
            {
                return false;
            }

            var notaPulita = nota.Trim();
            commessa.NoteInterne = string.IsNullOrWhiteSpace(commessa.NoteInterne)
                ? notaPulita
                : $"{commessa.NoteInterne}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {notaPulita}";

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
            return true;
        }

        public async Task<bool> LinkMisuraAsync(int id, int misuraClienteId, string? userId, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.CommissioniSartoriali.FirstOrDefaultAsync(c => c.Id == id);
            if (commessa == null)
            {
                return false;
            }

            if (!isAdmin && negozioId.HasValue && commessa.NegozioId != negozioId.Value)
            {
                return false;
            }

            var misura = await _context.RegistroMisure.FirstOrDefaultAsync(m => m.Id == misuraClienteId);
            if (misura == null || misura.ClienteId != commessa.ClienteId)
            {
                return false;
            }

            var exists = await _context.CommissioniMisureLinks
                .AnyAsync(x => x.CommessaSartorialeId == id && x.MisuraClienteId == misuraClienteId);
            if (exists)
            {
                return true;
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
            return true;
        }

        public async Task<bool> UnlinkMisuraAsync(int id, int misuraClienteId, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.CommissioniSartoriali.FirstOrDefaultAsync(c => c.Id == id);
            if (commessa == null)
            {
                return false;
            }

            if (!isAdmin && negozioId.HasValue && commessa.NegozioId != negozioId.Value)
            {
                return false;
            }

            var link = await _context.CommissioniMisureLinks
                .FirstOrDefaultAsync(x => x.CommessaSartorialeId == id && x.MisuraClienteId == misuraClienteId);
            if (link == null)
            {
                return false;
            }

            _context.CommissioniMisureLinks.Remove(link);

            _context.CommissioniEventi.Add(new CommessaEvento
            {
                CommessaSartorialeId = id,
                TipoEvento = "UnlinkMisura",
                Descrizione = $"Scollegata misura id {misuraClienteId}.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
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

        public async Task<CommessaMisuraStatus> GetStatoMisureClienteAsync(int commessaId, int? negozioId, bool isAdmin)
        {
            var commessa = await _context.CommissioniSartoriali
                .AsNoTracking()
                .Select(c => new { c.Id, c.ClienteId, c.NegozioId })
                .FirstOrDefaultAsync(c => c.Id == commessaId);

            if (commessa == null)
            {
                return new CommessaMisuraStatus();
            }

            if (!isAdmin && negozioId.HasValue && commessa.NegozioId != negozioId.Value)
            {
                return new CommessaMisuraStatus();
            }

            var totaleMisureCliente = await _context.RegistroMisure
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
