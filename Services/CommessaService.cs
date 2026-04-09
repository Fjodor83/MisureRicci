using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public class CommessaService : ICommessaService
    {
        private readonly ICommessaQueryService _query;
        private readonly ICommessaCommandService _command;
        private readonly ICommessaMisuraLinkService _link;

        public CommessaService(
            ICommessaQueryService query,
            ICommessaCommandService command,
            ICommessaMisuraLinkService link)
        {
            _query = query;
            _command = command;
            _link = link;
        }

        public Task<PagedResult<CommessaSartoriale>> GetCommissioniPagedAsync(
            int? clienteId, int? negozioId, bool isAdmin, int page, int pageSize)
            => _query.GetCommissioniPagedAsync(clienteId, negozioId, isAdmin, page, pageSize);

        public Task<CommessaKpiViewModel> GetKpiAsync(int? negozioId, bool isAdmin)
            => _query.GetKpiAsync(negozioId, isAdmin);

        public Task<CommessaSartoriale?> GetCommessaByIdAsync(int id, int? negozioId, bool isAdmin)
            => _query.GetCommessaByIdAsync(id, negozioId, isAdmin);

        public Task<CommessaDetailsViewModel?> GetCommessaDetailsAsync(int id, int? negozioId, bool isAdmin)
            => _query.GetCommessaDetailsAsync(id, negozioId, isAdmin);

        public Task<List<CommessaMisuraItem>> GetMisureDisponibiliPerClienteAsync(
            int clienteId, int? negozioId, bool isAdmin)
            => _query.GetMisureDisponibiliPerClienteAsync(clienteId, negozioId, isAdmin);

        public Task<CommessaMisuraStatus> GetStatoMisureClienteAsync(
            int commessaId, int? negozioId, bool isAdmin)
            => _query.GetStatoMisureClienteAsync(commessaId, negozioId, isAdmin);

        public Task<Result<CommessaSartoriale>> CreateCommessaAsync(
            CommessaCreateViewModel model, string? userId, int? negozioId, bool isAdmin)
            => _command.CreateCommessaAsync(model, userId, negozioId, isAdmin);

        public Task<Result> DeleteCommessaAsync(int id, int? negozioId, bool isAdmin)
            => _command.DeleteCommessaAsync(id, negozioId, isAdmin);

        public Task<Result<int>> CreateAndLinkDynamicMeasurementAsync(
            DynamicMeasurementCreateViewModel model, string? userId, int? negozioId, bool isAdmin)
            => _command.CreateAndLinkDynamicMeasurementAsync(model, userId, negozioId, isAdmin);

        public Task<Result> AdvanceStatoAsync(
            int id, StatoCommessa nuovoStato, string? note, string? userId, int? negozioId, bool isAdmin)
            => _command.AdvanceStatoAsync(id, nuovoStato, note, userId, negozioId, isAdmin);

        public Task<Result> AddNotaAsync(int id, string nota, string? userId, int? negozioId, bool isAdmin)
            => _command.AddNotaAsync(id, nota, userId, negozioId, isAdmin);

        public Task<Result> LinkMisuraAsync(
            int id, int misuraClienteId, string? userId, int? negozioId, bool isAdmin)
            => _link.LinkMisuraAsync(id, misuraClienteId, userId, negozioId, isAdmin);

        public Task<bool> LinkDynamicMeasurementRecordAsync(
            int id, int dynamicRecordId, string? userId, int? negozioId, bool isAdmin)
            => _link.LinkDynamicMeasurementRecordAsync(id, dynamicRecordId, userId, negozioId, isAdmin);

        public Task<Result> LinkDynamicMeasurementRecordInternalAsync(
            int id, int dynamicRecordId, string? userId, int? negozioId, bool isAdmin)
            => _link.LinkDynamicMeasurementRecordInternalAsync(id, dynamicRecordId, userId, negozioId, isAdmin);

        public Task<Result> UnlinkMisuraAsync(int id, int misuraClienteId, int? negozioId, bool isAdmin)
            => _link.UnlinkMisuraAsync(id, misuraClienteId, negozioId, isAdmin);
    }
}
