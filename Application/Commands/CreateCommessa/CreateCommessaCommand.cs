using MisureRicci.Models.ViewModels;

namespace MisureRicci.Application.Commands.CreateCommessa;

public sealed record CreateCommessaCommand(
    CommessaCreateViewModel Payload,
    string? UserId,
    int? NegozioId,
    bool IsAdmin);
