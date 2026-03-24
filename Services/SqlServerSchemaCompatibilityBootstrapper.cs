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
-- 0. Ensure Migration History exists
IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

-- 1. Mark InitialCreate as applied if AspNetRoles exists
IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NOT NULL 
   AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260323150111_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260323150111_InitialCreate', '8.0.0');
END;

-- 2. Mark AddMeasurementTypeImageUrl as applied if ImageUrl exists in MeasurementTypes or DynamicMeasurementTypes
IF (COL_LENGTH(N'dbo.MeasurementTypes', N'ImageUrl') IS NOT NULL OR COL_LENGTH(N'dbo.DynamicMeasurementTypes', N'ImageUrl') IS NOT NULL)
   AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260324092407_AddMeasurementTypeImageUrl')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260324092407_AddMeasurementTypeImageUrl', '8.0.0');
END;

-- 3. Mark NewTables as applied if DynamicMeasurementTypes exists
IF OBJECT_ID(N'[dbo].[DynamicMeasurementTypes]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260324131120_NewTables')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260324131120_NewTables', '8.0.0');
END;

-- 4. Check for legacy Dynamic Measurement tables that might need renaming ONLY if NewTables is NOT marked as applied
-- This handles cases where the tables were created manually or by a different process but not yet tracked by EF.
IF OBJECT_ID(N'[dbo].[MeasurementTypes]', N'U') IS NOT NULL 
   AND OBJECT_ID(N'[dbo].[DynamicMeasurementTypes]', N'U') IS NULL
   AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260324131120_NewTables')
BEGIN
    EXEC sp_rename 'MeasurementTypes', 'DynamicMeasurementTypes';
END;

IF OBJECT_ID(N'[dbo].[MeasurementFieldDefinitions]', N'U') IS NOT NULL 
   AND OBJECT_ID(N'[dbo].[DynamicFieldDefinitions]', N'U') IS NULL
   AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260324131120_NewTables')
BEGIN
    EXEC sp_rename 'MeasurementFieldDefinitions', 'DynamicFieldDefinitions';
END;

-- 5. Check for other column updates
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
