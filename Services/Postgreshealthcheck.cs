using Npgsql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MisureRicci.Services
{
    /// <summary>
    /// Health check per PostgreSQL (Railway).
    /// Rimpiazza SqlServerHealthCheck non più necessario.
    /// </summary>
    public class PostgresHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;

        public PostgresHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return HealthCheckResult.Unhealthy("DefaultConnection non configurata.");
            }

            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT 1";
                await cmd.ExecuteScalarAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Connessione PostgreSQL non disponibile.", ex);
            }
        }
    }
}
