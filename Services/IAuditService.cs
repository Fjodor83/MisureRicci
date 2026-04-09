using MisureRicci.Models;

namespace MisureRicci.Services
{
    public interface IAuditService
    {
        Task WriteAsync(AuditLog entry, CancellationToken ct = default);

        Task WriteAsync(
            string entityName,
            string? entityId,
            string action,
            string? userId,
            string? oldValues,
            string? newValues,
            CancellationToken ct = default);
    }
}
