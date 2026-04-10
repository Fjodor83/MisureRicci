using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public interface IAuditLogQueryService
    {
        Task<IReadOnlyList<AuditLog>> GetLatestAsync(CancellationToken cancellationToken = default);
    }

    public class AuditLogQueryService : IAuditLogQueryService
    {
        private const int ActivityLogPageSize = 50;
        private readonly ApplicationDbContext _context;

        public AuditLogQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AuditLog>> GetLatestAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(entry => entry.Timestamp)
                .Take(ActivityLogPageSize)
                .ToListAsync(cancellationToken);
        }
    }
}