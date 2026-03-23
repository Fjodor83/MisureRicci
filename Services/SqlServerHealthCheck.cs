using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MisureRicci.Services
{
    public class SqlServerHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;

        public SqlServerHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return HealthCheckResult.Unhealthy("DefaultConnection non configurata.");
            }

            try
            {
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Connessione SQL Server non disponibile.", ex);
            }
        }
    }
}
