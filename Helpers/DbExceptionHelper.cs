using Microsoft.EntityFrameworkCore;

namespace MisureRicci.Helpers
{
    /// <summary>
    /// Helper condiviso per diagnosticare eccezioni database.
    /// Usa i codici di errore SQL Server invece del testo del messaggio (locale-independent).
    /// </summary>
    public static class DbExceptionHelper
    {
        /// <summary>
        /// Rileva se una <see cref="DbUpdateException"/> è causata da una violazione
        /// di vincolo UNIQUE (SQL Server error 2601) o PRIMARY KEY/UNIQUE (error 2627).
        /// </summary>
        public static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            var sqlEx = ex.InnerException as Microsoft.Data.SqlClient.SqlException;
            return sqlEx?.Number is 2601 or 2627;
        }
    }
}
