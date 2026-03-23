using Microsoft.EntityFrameworkCore;
using MisureRicci.Data;

namespace MisureRicci.Services
{
    public static class SqlServerSchemaCompatibilityBootstrapper
    {
        private const string ClientCodeExpression = "CAST(N'SR-' + CONVERT(nvarchar(4), DATEPART(year, [DataRegistrazione])) + N'-' + RIGHT(N'00000' + CONVERT(nvarchar(5), [Id]), 5) AS nvarchar(20))";

        public static async Task EnsureCompatibleAsync(ApplicationDbContext dbContext)
        {
            if (!dbContext.Database.IsSqlServer())
            {
                return;
            }

            const string sql = @"
IF OBJECT_ID(N'[dbo].[RegistroMisure]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.RegistroMisure', N'IsDynamic') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RegistroMisure]
        ADD [IsDynamic] bit NOT NULL
            CONSTRAINT [DF_RegistroMisure_IsDynamic] DEFAULT (0) WITH VALUES;
    END;

    IF COL_LENGTH(N'dbo.RegistroMisure', N'SystemNote') IS NULL
    BEGIN
        ALTER TABLE [dbo].[RegistroMisure]
        ADD [SystemNote] nvarchar(max) NULL;
    END;
END;

IF OBJECT_ID(N'[dbo].[Clienti]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Clienti', N'ClientCode') IS NULL
    BEGIN
        ALTER TABLE [dbo].[Clienti]
        ADD [ClientCode] AS " + ClientCodeExpression + @" PERSISTED;
    END;
    ELSE IF EXISTS (
            SELECT 1
            FROM sys.columns
            WHERE object_id = OBJECT_ID(N'[dbo].[Clienti]')
              AND name = N'ClientCode'
              AND (max_length = -1 OR max_length > 900))
       AND NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_Clienti_ClientCode'
              AND object_id = OBJECT_ID(N'[dbo].[Clienti]'))
    BEGIN
        ALTER TABLE [dbo].[Clienti] DROP COLUMN [ClientCode];

        ALTER TABLE [dbo].[Clienti]
        ADD [ClientCode] AS " + ClientCodeExpression + @" PERSISTED;
    END;

    IF EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_Clienti_ClientCode'
              AND object_id = OBJECT_ID(N'[dbo].[Clienti]')
              AND has_filter = 1)
    BEGIN
        DROP INDEX [IX_Clienti_ClientCode] ON [dbo].[Clienti];
    END;

    IF COL_LENGTH(N'dbo.Clienti', N'ClientCode') IS NOT NULL
       AND NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_Clienti_ClientCode'
              AND object_id = OBJECT_ID(N'[dbo].[Clienti]'))
    BEGIN
        CREATE UNIQUE INDEX [IX_Clienti_ClientCode]
        ON [dbo].[Clienti]([ClientCode]);
    END;
END;

IF OBJECT_ID(N'[dbo].[AspNetUsers]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AspNetUsers', N'Attivo') IS NULL
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [Attivo] bit NOT NULL
        CONSTRAINT [DF_AspNetUsers_Attivo] DEFAULT (1) WITH VALUES;
END;";

            await dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
