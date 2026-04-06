#!/usr/bin/env pwsh
# ============================================================
# Script da eseguire UNA SOLA VOLTA in locale
# Prima di fare il primo deploy su Railway con PostgreSQL
# ============================================================

Write-Host "=== MisureRicci: Migrazione da SQL Server a PostgreSQL ===" -ForegroundColor Cyan

# 1. Rimuovi le vecchie migration SQL Server
Write-Host "`n[1/4] Elimino le migration SQL Server esistenti..." -ForegroundColor Yellow
if (Test-Path "Migrations") {
    Remove-Item -Recurse -Force "Migrations"
    Write-Host "     Cartella Migrations eliminata." -ForegroundColor Green
}

# 2. Installa dotnet-ef se non presente
Write-Host "`n[2/4] Verifico dotnet-ef..." -ForegroundColor Yellow
$efVersion = dotnet ef --version 2>$null
if (-not $efVersion) {
    Write-Host "     Installo dotnet-ef..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}
Write-Host "     dotnet-ef: OK" -ForegroundColor Green

# 3. Imposta la connection string locale verso un PostgreSQL di test
# Puoi usare Docker: docker run -e POSTGRES_PASSWORD=dev -p 5432:5432 postgres:16
Write-Host "`n[3/4] Imposta connessione PostgreSQL locale (User Secrets)..." -ForegroundColor Yellow
Write-Host "     Assicurati di avere PostgreSQL in ascolto su localhost:5432" -ForegroundColor Gray
Write-Host "     Poi imposta la connection string:" -ForegroundColor Gray
Write-Host ""
Write-Host '     dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=misurericci_dev;Username=postgres;Password=dev"' -ForegroundColor White
Write-Host ""

# 4. Crea la nuova migration
Write-Host "[4/4] Creo la migration InitialCreate per PostgreSQL..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate
if ($LASTEXITCODE -eq 0) {
    Write-Host "     Migration creata con successo!" -ForegroundColor Green
} else {
    Write-Host "     ERRORE nella creazione migration. Controlla i log sopra." -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Fatto! ===" -ForegroundColor Cyan
Write-Host "Ora puoi fare 'git push' e Railway applicherà le migration automaticamente." -ForegroundColor Green
Write-Host ""
Write-Host "VARIABILI DA IMPOSTARE SU RAILWAY:" -ForegroundColor Yellow
Write-Host "  DATABASE_URL          → automatica dal plugin PostgreSQL" -ForegroundColor White
Write-Host "  BootstrapAdmin__Enabled  → true" -ForegroundColor White
Write-Host "  BootstrapAdmin__Email    → tua-email@dominio.com" -ForegroundColor White
Write-Host "  BootstrapAdmin__Password → PasswordForte123!" -ForegroundColor White
Write-Host "  BootstrapAdmin__NomeCompleto → Amministratore" -ForegroundColor White
Write-Host "  ASPNETCORE_ENVIRONMENT → Production" -ForegroundColor White
