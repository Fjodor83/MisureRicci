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

-- 1. Ensure AspNetUsers has required columns for the new model
IF OBJECT_ID(N'[dbo].[AspNetUsers]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.AspNetUsers', N'NomeCompleto') IS NULL
        ALTER TABLE [dbo].[AspNetUsers] ADD [NomeCompleto] nvarchar(max) NOT NULL DEFAULT '';
    IF COL_LENGTH(N'dbo.AspNetUsers', N'Ruolo') IS NULL
        ALTER TABLE [dbo].[AspNetUsers] ADD [Ruolo] nvarchar(max) NOT NULL DEFAULT 'Sartoria';
    IF COL_LENGTH(N'dbo.AspNetUsers', N'NegozioId') IS NULL
        ALTER TABLE [dbo].[AspNetUsers] ADD [NegozioId] int NULL;
    IF COL_LENGTH(N'dbo.AspNetUsers', N'Attivo') IS NULL
        ALTER TABLE [dbo].[AspNetUsers] ADD [Attivo] bit NOT NULL DEFAULT 1;
END

-- 2. Ensure Negozi exists (required for users and clients)
IF OBJECT_ID(N'[dbo].[Negozi]', N'U') IS NULL
BEGIN
    CREATE TABLE [Negozi] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(max) NOT NULL,
        [Citta] nvarchar(max) NOT NULL,
        [Indirizzo] nvarchar(max) NULL,
        [CodiceNegozio] nvarchar(max) NULL,
        [Paese] nvarchar(max) NOT NULL,
        [Attivo] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Negozi] PRIMARY KEY ([Id])
    );
END

-- 3. Dynamic Measurement Tables and Columns
IF OBJECT_ID(N'[dbo].[DynamicMeasurementTypes]', N'U') IS NULL
BEGIN
    IF OBJECT_ID(N'[dbo].[MeasurementTypes]', N'U') IS NOT NULL
        EXEC sp_rename 'MeasurementTypes', 'DynamicMeasurementTypes';
    ELSE
    BEGIN
        CREATE TABLE [DynamicMeasurementTypes] (
            [Id] int NOT NULL IDENTITY,
            [Nome] nvarchar(80) NOT NULL,
            [Descrizione] nvarchar(250) NULL,
            [IsActive] bit NOT NULL DEFAULT 1,
            [IsSystem] bit NOT NULL DEFAULT 0,
            [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
            [ImageUrl] nvarchar(500) NULL,
            CONSTRAINT [PK_DynamicMeasurementTypes] PRIMARY KEY ([Id])
        );
    END
END

-- Ensure columns exist in DynamicMeasurementTypes
IF OBJECT_ID(N'[dbo].[DynamicMeasurementTypes]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.DynamicMeasurementTypes', N'ImageUrl') IS NULL
        ALTER TABLE [dbo].[DynamicMeasurementTypes] ADD [ImageUrl] nvarchar(500) NULL;
    IF COL_LENGTH(N'dbo.DynamicMeasurementTypes', N'Descrizione') IS NULL
        ALTER TABLE [dbo].[DynamicMeasurementTypes] ADD [Descrizione] nvarchar(250) NULL;
    IF COL_LENGTH(N'dbo.DynamicMeasurementTypes', N'IsActive') IS NULL
        ALTER TABLE [dbo].[DynamicMeasurementTypes] ADD [IsActive] bit NOT NULL DEFAULT 1;
    IF COL_LENGTH(N'dbo.DynamicMeasurementTypes', N'IsSystem') IS NULL
        ALTER TABLE [dbo].[DynamicMeasurementTypes] ADD [IsSystem] bit NOT NULL DEFAULT 0;
    IF COL_LENGTH(N'dbo.DynamicMeasurementTypes', N'CreatedAt') IS NULL
        ALTER TABLE [dbo].[DynamicMeasurementTypes] ADD [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
END

IF OBJECT_ID(N'[dbo].[DynamicFieldDefinitions]', N'U') IS NULL
BEGIN
    IF OBJECT_ID(N'[dbo].[MeasurementFieldDefinitions]', N'U') IS NOT NULL
        EXEC sp_rename 'MeasurementFieldDefinitions', 'DynamicFieldDefinitions';
    ELSE
    BEGIN
        CREATE TABLE [DynamicFieldDefinitions] (
            [Id] int NOT NULL IDENTITY,
            [MeasurementTypeId] int NOT NULL,
            [NomeCampo] nvarchar(80) NOT NULL,
            [Etichetta] nvarchar(100) NOT NULL,
            [Gruppo] nvarchar(80) NULL,
            [OrdineGruppo] int NOT NULL DEFAULT 0,
            [TipoDato] int NOT NULL DEFAULT 0,
            [Template] int NOT NULL DEFAULT 0,
            [UnitaMisura] nvarchar(20) NULL,
            [Placeholder] nvarchar(120) NULL,
            [HelpText] nvarchar(160) NULL,
            [Obbligatorio] bit NOT NULL DEFAULT 0,
            [Ordine] int NOT NULL DEFAULT 0,
            [IsActive] bit NOT NULL DEFAULT 1,
            CONSTRAINT [PK_DynamicFieldDefinitions] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_DynamicFieldDefinitions_DynamicMeasurementTypes_MeasurementTypeId] FOREIGN KEY ([MeasurementTypeId]) REFERENCES [DynamicMeasurementTypes] ([Id]) ON DELETE CASCADE
        );
    END
END

IF OBJECT_ID(N'[dbo].[DynamicMeasurementRecords]', N'U') IS NULL
BEGIN
    CREATE TABLE [DynamicMeasurementRecords] (
        [Id] int NOT NULL IDENTITY,
        [ClienteId] int NOT NULL,
        [MeasurementTypeId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedByUserId] nvarchar(450) NULL,
        CONSTRAINT [PK_DynamicMeasurementRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DynamicMeasurementRecords_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DynamicMeasurementRecords_DynamicMeasurementTypes_MeasurementTypeId] FOREIGN KEY ([MeasurementTypeId]) REFERENCES [DynamicMeasurementTypes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DynamicMeasurementRecords_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id])
    );
END

IF OBJECT_ID(N'[dbo].[DynamicMeasurementValues]', N'U') IS NULL
BEGIN
    CREATE TABLE [DynamicMeasurementValues] (
        [Id] int NOT NULL IDENTITY,
        [DynamicMeasurementRecordId] int NOT NULL,
        [MeasurementFieldDefinitionId] int NOT NULL,
        [Valore] nvarchar(1000) NULL,
        CONSTRAINT [PK_DynamicMeasurementValues] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DynamicMeasurementValues_DynamicFieldDefinitions_MeasurementFieldDefinitionId] FOREIGN KEY ([MeasurementFieldDefinitionId]) REFERENCES [DynamicFieldDefinitions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DynamicMeasurementValues_DynamicMeasurementRecords_DynamicMeasurementRecordId] FOREIGN KEY ([DynamicMeasurementRecordId]) REFERENCES [DynamicMeasurementRecords] ([Id]) ON DELETE CASCADE
    );
END

-- 4. Registry and Legacy Columns
IF OBJECT_ID(N'[dbo].[RegistroMisure]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.RegistroMisure', N'IsDynamic') IS NULL
        ALTER TABLE [dbo].[RegistroMisure] ADD [IsDynamic] bit NOT NULL DEFAULT 0;
    IF COL_LENGTH(N'dbo.RegistroMisure', N'SystemNote') IS NULL
        ALTER TABLE [dbo].[RegistroMisure] ADD [SystemNote] nvarchar(max) NULL;
END

IF OBJECT_ID(N'[dbo].[Clienti]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Clienti', N'ClientCode') IS NULL
        ALTER TABLE [dbo].[Clienti] ADD [ClientCode] AS " + ClientCodeExpression + @" PERSISTED;
END

-- 5. Mark InitialCreate as applied if AspNetRoles exists
IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NOT NULL 
   AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260325110742_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260325110742_InitialCreate', '8.0.0');
END;
";

            await dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
