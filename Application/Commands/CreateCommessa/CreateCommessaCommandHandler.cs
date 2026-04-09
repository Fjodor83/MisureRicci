using MisureRicci.Models;
using MisureRicci.Services;

namespace MisureRicci.Application.Commands.CreateCommessa;

public sealed class CreateCommessaCommandHandler
{
    private readonly ICommessaService _commessaService;

    public CreateCommessaCommandHandler(ICommessaService commessaService)
    {
        _commessaService = commessaService;
    }

    public Task<Result<CommessaSartoriale>> HandleAsync(CreateCommessaCommand command, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return _commessaService.CreateCommessaAsync(
            command.Payload,
            command.UserId,
            command.NegozioId,
            command.IsAdmin);
    }
}
