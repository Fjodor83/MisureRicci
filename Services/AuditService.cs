using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task WriteAsync(AuditLog entry, CancellationToken ct = default)
        {
            if (entry == null)
            {
                return;
            }

            if (entry.Timestamp == default)
            {
                entry.Timestamp = DateTime.UtcNow;
            }

            _context.AuditLogs.Add(entry);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                // Stub behavior: audit failures are logged but do not block business flow.
                _logger.LogWarning(ex, "Audit log write failed for {EntityName}:{EntityId}", entry.EntityName, entry.EntityId);
            }
        }

        public Task WriteAsync(
            string entityName,
            string? entityId,
            string action,
            string? userId,
            string? oldValues,
            string? newValues,
            CancellationToken ct = default)
        {
            return WriteAsync(new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                OldValues = oldValues,
                NewValues = newValues
            }, ct);
        }
    }
}
