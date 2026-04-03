 IF DB_ID(N'STEFFANO_RICCI_MISURE') IS NULL
BEGIN
    CREATE DATABASE [STEFFANO_RICCI_MISURE];
END
GO
USE [STEFFANO_RICCI_MISURE];
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [DynamicMeasurementTypes] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(80) NOT NULL,
    [Descrizione] nvarchar(250) NULL,
    [IsActive] bit NOT NULL,
    [IsSystem] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ImageUrl] nvarchar(500) NULL,
    CONSTRAINT [PK_DynamicMeasurementTypes] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Negozi] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(max) NOT NULL,
    [Citta] nvarchar(max) NOT NULL,
    [Indirizzo] nvarchar(max) NULL,
    [CodiceNegozio] nvarchar(max) NULL,
    [Paese] nvarchar(max) NOT NULL,
    [Attivo] bit NOT NULL,
    CONSTRAINT [PK_Negozi] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [DynamicFieldDefinitions] (
    [Id] int NOT NULL IDENTITY,
    [MeasurementTypeId] int NOT NULL,
    [NomeCampo] nvarchar(80) NOT NULL,
    [Etichetta] nvarchar(100) NOT NULL,
    [Gruppo] nvarchar(80) NULL,
    [OrdineGruppo] int NOT NULL,
    [TipoDato] int NOT NULL,
    [Template] int NOT NULL,
    [UnitaMisura] nvarchar(20) NULL,
    [Placeholder] nvarchar(120) NULL,
    [HelpText] nvarchar(160) NULL,
    [Obbligatorio] bit NOT NULL,
    [Ordine] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_DynamicFieldDefinitions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DynamicFieldDefinitions_DynamicMeasurementTypes_MeasurementTypeId] FOREIGN KEY ([MeasurementTypeId]) REFERENCES [DynamicMeasurementTypes] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [NomeCompleto] nvarchar(max) NOT NULL,
    [Ruolo] nvarchar(max) NOT NULL,
    [NegozioId] int NULL,
    [Attivo] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_Negozi_NegozioId] FOREIGN KEY ([NegozioId]) REFERENCES [Negozi] ([Id])
);
GO

CREATE TABLE [Clienti] (
    [Id] int NOT NULL IDENTITY,
    [ClientCode] nvarchar(max) NULL,
    [Nome] nvarchar(max) NOT NULL,
    [Cognome] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Telefono] nvarchar(max) NULL,
    [Indirizzo] nvarchar(max) NULL,
    [Citta] nvarchar(max) NULL,
    [StatoProvincia] nvarchar(max) NULL,
    [CodicePostale] nvarchar(max) NULL,
    [Paese] nvarchar(max) NOT NULL,
    [Note] nvarchar(max) NULL,
    [DataRegistrazione] datetime2 NOT NULL,
    [NegozioId] int NULL,
    CONSTRAINT [PK_Clienti] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Clienti_Negozi_NegozioId] FOREIGN KEY ([NegozioId]) REFERENCES [Negozi] ([Id])
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CommissioniSartoriali] (
    [Id] int NOT NULL IDENTITY,
    [CommessaCode] nvarchar(30) NULL,
    [ClienteId] int NOT NULL,
    [NegozioId] int NULL,
    [TipoCapo] nvarchar(80) NOT NULL,
    [Tessuto] nvarchar(120) NULL,
    [Collezione] nvarchar(120) NULL,
    [DataApertura] datetime2 NOT NULL,
    [DataConsegnaPrevista] datetime2 NULL,
    [DataConsegnaEffettiva] datetime2 NULL,
    [Stato] int NOT NULL,
    [NoteInterne] nvarchar(2000) NULL,
    [CreatedByUserId] nvarchar(450) NULL,
    CONSTRAINT [PK_CommissioniSartoriali] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CommissioniSartoriali_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_CommissioniSartoriali_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CommissioniSartoriali_Negozi_NegozioId] FOREIGN KEY ([NegozioId]) REFERENCES [Negozi] ([Id])
);
GO

CREATE TABLE [DynamicMeasurementRecords] (
    [Id] int NOT NULL IDENTITY,
    [ClienteId] int NOT NULL,
    [MeasurementTypeId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedByUserId] nvarchar(450) NULL,
    [MeasurementUnit] int NOT NULL,
    CONSTRAINT [PK_DynamicMeasurementRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DynamicMeasurementRecords_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_DynamicMeasurementRecords_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DynamicMeasurementRecords_DynamicMeasurementTypes_MeasurementTypeId] FOREIGN KEY ([MeasurementTypeId]) REFERENCES [DynamicMeasurementTypes] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MisureCamicia] (
    [Id] int NOT NULL IDENTITY,
    [Collo] float NOT NULL,
    [Spalle] float NOT NULL,
    [Torace] float NOT NULL,
    [Vita] float NOT NULL,
    [Manica] float NOT NULL,
    [Polso] float NOT NULL,
    [Lunghezza] float NOT NULL,
    [ClienteId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [OrderId] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_MisureCamicia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MisureCamicia_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MisureCintura] (
    [Id] int NOT NULL IDENTITY,
    [Lunghezza] float NOT NULL,
    [Girovita] float NOT NULL,
    [ClienteId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [OrderId] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_MisureCintura] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MisureCintura_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MisureCravatta] (
    [Id] int NOT NULL IDENTITY,
    [Lunghezza] float NOT NULL,
    [Larghezza] float NOT NULL,
    [ClienteId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [OrderId] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_MisureCravatta] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MisureCravatta_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MisureGiacca] (
    [Id] int NOT NULL IDENTITY,
    [Spalle] float NOT NULL,
    [Torace] float NOT NULL,
    [Vita] float NOT NULL,
    [Manica] float NOT NULL,
    [Lunghezza] float NOT NULL,
    [ClienteId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [OrderId] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_MisureGiacca] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MisureGiacca_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MisureGilet] (
    [Id] int NOT NULL IDENTITY,
    [Torace] float NOT NULL,
    [Vita] float NOT NULL,
    [Lunghezza] float NOT NULL,
    [ClienteId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [OrderId] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_MisureGilet] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MisureGilet_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MisureMaglie] (
    [Id] int NOT NULL IDENTITY,
    [Torace] float NOT NULL,
    [Spalle] float NOT NULL,
    [Manica] float NOT NULL,
    [Lunghezza] float NOT NULL,
    [ClienteId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [OrderId] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_MisureMaglie] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MisureMaglie_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MisureOutdoor] (
    [Id] int NOT NULL IDENTITY,
    [Torace] float NOT NULL,
    [Spalle] float NOT NULL,
    [Manica] float NOT NULL,
    [Lunghezza] float NOT NULL,
    [Fit] nvarchar(max) NULL,
    [ClienteId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [OrderId] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_MisureOutdoor] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MisureOutdoor_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MisurePantalone] (
    [Id] int NOT NULL IDENTITY,
    [Vita] float NOT NULL,
    [Bacino] float NOT NULL,
    [Cavallo] float NOT NULL,
    [InternoGamba] float NOT NULL,
    [Fondo] float NOT NULL,
    [ClienteId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [OrderId] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_MisurePantalone] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MisurePantalone_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MisureScarpe] (
    [Id] int NOT NULL IDENTITY,
    [Taglia] nvarchar(max) NULL,
    [LunghezzaPiede] float NOT NULL,
    [Pianta] nvarchar(max) NULL,
    [ClienteId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [OrderId] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_MisureScarpe] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MisureScarpe_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RegistroMisure] (
    [Id] int NOT NULL IDENTITY,
    [ClienteId] int NOT NULL,
    [TipoMisura] nvarchar(max) NOT NULL,
    [DataCreazione] datetime2 NOT NULL,
    [Note] nvarchar(max) NULL,
    [SystemNote] nvarchar(max) NULL,
    [IsDynamic] bit NOT NULL,
    [RecordId] int NOT NULL,
    CONSTRAINT [PK_RegistroMisure] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RegistroMisure_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CommissioniEventi] (
    [Id] int NOT NULL IDENTITY,
    [CommessaSartorialeId] int NOT NULL,
    [TipoEvento] nvarchar(40) NOT NULL,
    [NuovoStato] int NULL,
    [Descrizione] nvarchar(1000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedByUserId] nvarchar(450) NULL,
    CONSTRAINT [PK_CommissioniEventi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CommissioniEventi_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_CommissioniEventi_CommissioniSartoriali_CommessaSartorialeId] FOREIGN KEY ([CommessaSartorialeId]) REFERENCES [CommissioniSartoriali] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [DynamicMeasurementValues] (
    [Id] int NOT NULL IDENTITY,
    [DynamicMeasurementRecordId] int NOT NULL,
    [MeasurementFieldDefinitionId] int NOT NULL,
    [Valore] nvarchar(1000) NULL,
    CONSTRAINT [PK_DynamicMeasurementValues] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DynamicMeasurementValues_DynamicFieldDefinitions_MeasurementFieldDefinitionId] FOREIGN KEY ([MeasurementFieldDefinitionId]) REFERENCES [DynamicFieldDefinitions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DynamicMeasurementValues_DynamicMeasurementRecords_DynamicMeasurementRecordId] FOREIGN KEY ([DynamicMeasurementRecordId]) REFERENCES [DynamicMeasurementRecords] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MisureAbitoCompleto] (
    [Id] int NOT NULL IDENTITY,
    [GiaccaId] int NOT NULL,
    [PantaloneId] int NOT NULL,
    [ClienteId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [OrderId] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_MisureAbitoCompleto] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MisureAbitoCompleto_Clienti_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clienti] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MisureAbitoCompleto_MisureGiacca_GiaccaId] FOREIGN KEY ([GiaccaId]) REFERENCES [MisureGiacca] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MisureAbitoCompleto_MisurePantalone_PantaloneId] FOREIGN KEY ([PantaloneId]) REFERENCES [MisurePantalone] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [CommissioniMisureLinks] (
    [Id] int NOT NULL IDENTITY,
    [CommessaSartorialeId] int NOT NULL,
    [MisuraClienteId] int NOT NULL,
    [LinkedAt] datetime2 NOT NULL,
    [LinkedByUserId] nvarchar(450) NULL,
    CONSTRAINT [PK_CommissioniMisureLinks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CommissioniMisureLinks_AspNetUsers_LinkedByUserId] FOREIGN KEY ([LinkedByUserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_CommissioniMisureLinks_CommissioniSartoriali_CommessaSartorialeId] FOREIGN KEY ([CommessaSartorialeId]) REFERENCES [CommissioniSartoriali] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CommissioniMisureLinks_RegistroMisure_MisuraClienteId] FOREIGN KEY ([MisuraClienteId]) REFERENCES [RegistroMisure] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE INDEX [IX_AspNetUsers_NegozioId] ON [AspNetUsers] ([NegozioId]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Clienti_NegozioId] ON [Clienti] ([NegozioId]);
GO

CREATE INDEX [IX_CommissioniEventi_CommessaSartorialeId] ON [CommissioniEventi] ([CommessaSartorialeId]);
GO

CREATE INDEX [IX_CommissioniEventi_CreatedByUserId] ON [CommissioniEventi] ([CreatedByUserId]);
GO

CREATE INDEX [IX_CommissioniMisureLinks_CommessaSartorialeId] ON [CommissioniMisureLinks] ([CommessaSartorialeId]);
GO

CREATE INDEX [IX_CommissioniMisureLinks_LinkedByUserId] ON [CommissioniMisureLinks] ([LinkedByUserId]);
GO

CREATE INDEX [IX_CommissioniMisureLinks_MisuraClienteId] ON [CommissioniMisureLinks] ([MisuraClienteId]);
GO

CREATE INDEX [IX_CommissioniSartoriali_ClienteId] ON [CommissioniSartoriali] ([ClienteId]);
GO

CREATE INDEX [IX_CommissioniSartoriali_CreatedByUserId] ON [CommissioniSartoriali] ([CreatedByUserId]);
GO

CREATE INDEX [IX_CommissioniSartoriali_NegozioId] ON [CommissioniSartoriali] ([NegozioId]);
GO

CREATE UNIQUE INDEX [IX_DynamicFieldDefinitions_MeasurementTypeId_NomeCampo] ON [DynamicFieldDefinitions] ([MeasurementTypeId], [NomeCampo]);
GO

CREATE INDEX [IX_DynamicMeasurementRecords_ClienteId] ON [DynamicMeasurementRecords] ([ClienteId]);
GO

CREATE INDEX [IX_DynamicMeasurementRecords_CreatedByUserId] ON [DynamicMeasurementRecords] ([CreatedByUserId]);
GO

CREATE INDEX [IX_DynamicMeasurementRecords_MeasurementTypeId] ON [DynamicMeasurementRecords] ([MeasurementTypeId]);
GO

CREATE UNIQUE INDEX [IX_DynamicMeasurementTypes_Nome] ON [DynamicMeasurementTypes] ([Nome]);
GO

CREATE INDEX [IX_DynamicMeasurementValues_DynamicMeasurementRecordId] ON [DynamicMeasurementValues] ([DynamicMeasurementRecordId]);
GO

CREATE INDEX [IX_DynamicMeasurementValues_MeasurementFieldDefinitionId] ON [DynamicMeasurementValues] ([MeasurementFieldDefinitionId]);
GO

CREATE INDEX [IX_MisureAbitoCompleto_ClienteId] ON [MisureAbitoCompleto] ([ClienteId]);
GO

CREATE INDEX [IX_MisureAbitoCompleto_GiaccaId] ON [MisureAbitoCompleto] ([GiaccaId]);
GO

CREATE INDEX [IX_MisureAbitoCompleto_PantaloneId] ON [MisureAbitoCompleto] ([PantaloneId]);
GO

CREATE INDEX [IX_MisureCamicia_ClienteId] ON [MisureCamicia] ([ClienteId]);
GO

CREATE INDEX [IX_MisureCintura_ClienteId] ON [MisureCintura] ([ClienteId]);
GO

CREATE INDEX [IX_MisureCravatta_ClienteId] ON [MisureCravatta] ([ClienteId]);
GO

CREATE INDEX [IX_MisureGiacca_ClienteId] ON [MisureGiacca] ([ClienteId]);
GO

CREATE INDEX [IX_MisureGilet_ClienteId] ON [MisureGilet] ([ClienteId]);
GO

CREATE INDEX [IX_MisureMaglie_ClienteId] ON [MisureMaglie] ([ClienteId]);
GO

CREATE INDEX [IX_MisureOutdoor_ClienteId] ON [MisureOutdoor] ([ClienteId]);
GO

CREATE INDEX [IX_MisurePantalone_ClienteId] ON [MisurePantalone] ([ClienteId]);
GO

CREATE INDEX [IX_MisureScarpe_ClienteId] ON [MisureScarpe] ([ClienteId]);
GO

CREATE INDEX [IX_RegistroMisure_ClienteId] ON [RegistroMisure] ([ClienteId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260403122809_InitialCreate', N'8.0.25');
GO

COMMIT;
GO



