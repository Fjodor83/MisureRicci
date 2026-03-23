# MisureRicci

## Setup rapido

### 1) Configura connessione DB
Aggiorna `ConnectionStrings:DefaultConnection` in `appsettings.json` oppure via variabile ambiente.

Per sviluppo locale (consigliato) usa User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=SERVER;Initial Catalog=DB;User ID=USER;Password=PASSWORD;Encrypt=False;TrustServerCertificate=True"
```

Oppure variabile ambiente:

```powershell
$env:ConnectionStrings__DefaultConnection="Data Source=SERVER;Initial Catalog=DB;User ID=USER;Password=PASSWORD;Encrypt=False;TrustServerCertificate=True"
```

### 2) Bootstrap admin sicuro (senza hardcode)
Il bootstrap admin e' gestito da configurazione in `BootstrapAdmin`.
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
