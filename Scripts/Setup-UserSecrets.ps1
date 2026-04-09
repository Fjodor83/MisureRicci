#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Configura i User Secrets per lo sviluppo locale di MisureRicci.
.DESCRIPTION
    Script interattivo per impostare ConnectionString, BootstrapAdmin e SMTP
    via dotnet user-secrets, evitando di salvare credenziali in file tracciati da Git.
.EXAMPLE
    .\Scripts\Setup-UserSecrets.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "`n=== MisureRicci - Setup User Secrets ===" -ForegroundColor Cyan
Write-Host "Questo script configura i segreti locali (dotnet user-secrets).`n"

# ── Connection String ──────────────────────────────────────────────
Write-Host "[1/3] Connessione al database" -ForegroundColor Yellow

$server   = Read-Host "  SQL Server (es. localhost\SQLEXPRESS)"
$database = Read-Host "  Database   (es. STEFFANO_RICCI_MISURE)"
$userId   = Read-Host "  User ID    (es. sa)"
$password = Read-Host "  Password" -AsSecureString
$plainPwd = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

$connString = "Data Source=$server;Initial Catalog=$database;User ID=$userId;Password=$plainPwd;Encrypt=False;TrustServerCertificate=True"

dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connString

Write-Host "  Connection string salvata.`n" -ForegroundColor Green

# ── Bootstrap Admin ────────────────────────────────────────────────
Write-Host "[2/3] Bootstrap Admin (opzionale)" -ForegroundColor Yellow
$setupAdmin = Read-Host "  Vuoi configurare il bootstrap admin? (s/N)"

if ($setupAdmin -eq 's' -or $setupAdmin -eq 'S') {
    $adminEmail = Read-Host "  Email admin"
    $adminPwd   = Read-Host "  Password admin"
    $adminNome  = Read-Host "  Nome completo"

    dotnet user-secrets set "BootstrapAdmin:Enabled"      "true"
    dotnet user-secrets set "BootstrapAdmin:Email"         $adminEmail
    dotnet user-secrets set "BootstrapAdmin:Password"      $adminPwd
    dotnet user-secrets set "BootstrapAdmin:NomeCompleto"  $adminNome

    Write-Host "  Bootstrap admin configurato.`n" -ForegroundColor Green
}
else {
    Write-Host "  Saltato.`n"
}

# ── SMTP ───────────────────────────────────────────────────────────
Write-Host "[3/3] SMTP (opzionale)" -ForegroundColor Yellow
$setupSmtp = Read-Host "  Vuoi configurare SMTP? (s/N)"

if ($setupSmtp -eq 's' -or $setupSmtp -eq 'S') {
    $smtpHost = Read-Host "  SMTP Host (es. localhost)"
    $smtpPort = Read-Host "  SMTP Port (es. 1025)"
    $smtpFrom = Read-Host "  From (es. dev@misurericci.local)"

    dotnet user-secrets set "Smtp:Host" $smtpHost
    dotnet user-secrets set "Smtp:Port" $smtpPort
    dotnet user-secrets set "Smtp:From" $smtpFrom

    Write-Host "  SMTP configurato.`n" -ForegroundColor Green
}
else {
    Write-Host "  Saltato.`n"
}

Write-Host "=== Setup completato ===" -ForegroundColor Cyan
Write-Host "Verifica con: dotnet user-secrets list`n"
