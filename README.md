# MisureRicci

Piattaforma sartoriale ASP.NET Core 8 MVC con EF Core, SQL Server, Identity, sistema di misure dinamiche e multi-tenancy per negozio.

## Setup rapido

### 1) Configura connessione DB (User Secrets — consigliato)

> **IMPORTANTE**: `appsettings.json` è in `.gitignore` e NON contiene credenziali.
> Usa **User Secrets** per lo sviluppo locale oppure variabili ambiente in produzione.

```powershell
# Inizializza User Secrets (se non già fatto)
dotnet user-secrets init

# Imposta la connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=SERVER;Initial Catalog=DB;User ID=USER;Password=PASSWORD;Encrypt=False;TrustServerCertificate=True"
```

Oppure variabile ambiente:

```powershell
$env:ConnectionStrings__DefaultConnection="Data Source=SERVER;Initial Catalog=DB;User ID=USER;Password=PASSWORD;Encrypt=False;TrustServerCertificate=True"
```

In alternativa puoi usare lo script pronto all'uso:

```powershell
.\Scripts\Setup-UserSecrets.ps1
```

### 2) Bootstrap admin sicuro (senza hardcode)
Il bootstrap admin è gestito da configurazione in `BootstrapAdmin`.
Per sviluppo locale, usa User Secrets:

```powershell
dotnet user-secrets set "BootstrapAdmin:Enabled" "true"
dotnet user-secrets set "BootstrapAdmin:Email" "admin@misure.ricci"
dotnet user-secrets set "BootstrapAdmin:Password" "SostituisciConPasswordForte123!"
dotnet user-secrets set "BootstrapAdmin:NomeCompleto" "Amministratore Sistema"
```

In produzione usa variabili ambiente (esempio):

```powershell
$env:BootstrapAdmin__Enabled="true"
$env:BootstrapAdmin__Email="admin@misure.ricci"
$env:BootstrapAdmin__Password="SostituisciConPasswordForte123!"
$env:BootstrapAdmin__NomeCompleto="Amministratore Sistema"
```

### 3) Build e test

```powershell
dotnet build MisureRicci.slnx
dotnet test MisureRicci.slnx
```

### 4) Configurazione storage file

Per sviluppo locale i file vengono salvati in `SecureUploads/` (provider `Local`).

Per produzione con Azure Blob Storage:
1. Imposta `Storage:Provider` a `AzureBlob` in `appsettings.Production.json` o variabile ambiente
2. Aggiungi il pacchetto NuGet `Azure.Storage.Blobs`
3. Configura connection string e container:

```json
{
  "Storage": {
    "Provider": "AzureBlob",
    "AzureBlobConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
    "ContainerName": "uploads"
  }
}
```

Oppure via User Secrets:
```powershell
dotnet user-secrets set "Storage:Provider" "AzureBlob"
dotnet user-secrets set "Storage:AzureBlobConnectionString" "..."
dotnet user-secrets set "Storage:ContainerName" "uploads"
```

### 5) Storage persistente PDF dossier

Il servizio PDF genera il dossier on-demand e tenta il salvataggio persistente tramite `IPdfStorageService`.

Provider attuale:
1. `LocalPdfStorageProvider`
2. Path default: `SecureUploads/PdfDossiers/`

Note operative:
1. In ambiente container il path deve puntare a un volume persistente.
2. Per storage esterno (es. Blob/S3) creare un nuovo provider che implementa `IPdfStorageService` e registrarlo in DI.

### 6) Evitare cancellazione file dopo update/deploy

Per mantenere i file caricati anche dopo restart o aggiornamenti:
1. Usa `Storage:Provider=Local`.
2. Imposta `Storage:LocalBasePath` su una cartella esterna al deploy dell'app.
3. Non usare cartelle sotto `bin/` o dentro la publish folder.

Esempio Windows:
```json
{
  "Storage": {
    "Provider": "Local",
    "LocalBasePath": "C:\\MisureRicciData"
  }
}
```

Esempio Docker (volume persistente):
```bash
docker run -d \
  -p 8080:8080 \
  -v misurericci_data:/data/misurericci \
  -e Storage__Provider=Local \
  -e Storage__LocalBasePath=/data/misurericci \
  misurericci:latest
```

Con Docker Compose:
1. Copia `.env.example` in `.env` e imposta la connection string.
2. Avvia con:

```bash
docker compose up -d --build
```

Il volume `misurericci_data` mantiene i file anche dopo restart e aggiornamenti container.
