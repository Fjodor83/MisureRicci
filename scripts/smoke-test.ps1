$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

param(
    [string]$AdminEmail = $env:SR_ADMIN_EMAIL,
    [string]$AdminPassword = $env:SR_ADMIN_PASSWORD,
    [string]$BaseUrl = $(if ($env:SR_BASE_URL) { $env:SR_BASE_URL } else { 'http://localhost:5129' })
)

if ([string]::IsNullOrWhiteSpace($AdminEmail) -or [string]::IsNullOrWhiteSpace($AdminPassword)) {
    throw 'Set SR_ADMIN_EMAIL and SR_ADMIN_PASSWORD (or pass -AdminEmail/-AdminPassword).'
}

$base = $BaseUrl
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

function Get-Token([string]$html) {
    foreach ($tag in [regex]::Matches($html, '<input\b[^>]*>').Value) {
        if ($tag -match 'name="__RequestVerificationToken"' -and $tag -match 'value="([^"]*)"') {
            return $matches[1]
        }
    }

    throw 'Request verification token not found.'
}

function Get-HiddenFields([string]$html) {
    $data = @{}
    foreach ($tag in [regex]::Matches($html, '<input\b[^>]*>').Value) {
        if ($tag -match 'type="hidden"' -and $tag -match 'name="([^"]+)"') {
            $name = $matches[1]
            $value = ''
            if ($tag -match 'value="([^"]*)"') {
                $value = $matches[1]
            }
            $data[$name] = $value
        }
    }

    return $data
}

function Get-ValueFields([string]$html) {
    $fields = @{}
    foreach ($tag in [regex]::Matches($html, '<(input|textarea|select)\b[^>]*>').Value) {
        if ($tag -match 'name="(Fields\[[0-9]+\]\.Valore)"') {
            $name = $matches[1]

            $fieldType = 'text'
            if ($tag -match '^<textarea\b') {
                $fieldType = 'textarea'
            }
            elseif ($tag -match '^<select\b') {
                $fieldType = 'select'
            }
            elseif ($tag -match 'type="([^"]+)"') {
                $fieldType = $matches[1].ToLowerInvariant()
            }

            if (-not $fields.ContainsKey($name) -or ($fields[$name] -eq 'hidden' -and $fieldType -ne 'hidden')) {
                $fields[$name] = $fieldType
            }
        }
    }

    return $fields.GetEnumerator() | Sort-Object Name
}

function Assert-Contains([string]$content, [string]$needle, [string]$message) {
    if ($content -notmatch [regex]::Escape($needle)) {
        throw $message
    }
}

function Strip-Html([string]$text) {
    if ([string]::IsNullOrWhiteSpace($text)) {
        return ''
    }

    $decoded = [System.Net.WebUtility]::HtmlDecode($text)
    $plain = [regex]::Replace($decoded, '<[^>]+>', ' ')
    return [regex]::Replace($plain, '\s+', ' ').Trim()
}

$loginPage = Invoke-WebRequest -UseBasicParsing -Uri "$base/Identity/Account/Login" -WebSession $session
$loginBody = @{
    '__RequestVerificationToken' = Get-Token $loginPage.Content
    'Input.Email' = $AdminEmail
    'Input.Password' = $AdminPassword
    'Input.RememberMe' = 'false'
}
$null = Invoke-WebRequest -UseBasicParsing -Uri "$base/Identity/Account/Login" -Method Post -WebSession $session -Body $loginBody -ContentType 'application/x-www-form-urlencoded'

$homePage = Invoke-WebRequest -UseBasicParsing -Uri "$base/" -WebSession $session
Assert-Contains $homePage.Content 'ADMIN' 'Admin login failed.'

$stamp = Get-Date -Format 'yyyyMMddHHmmss'
$cognome = "Smoke$stamp"

$clientPage = Invoke-WebRequest -UseBasicParsing -Uri "$base/Clienti/Create" -WebSession $session
$clientBody = @{
    '__RequestVerificationToken' = Get-Token $clientPage.Content
    'Nome' = 'E2E'
    'Cognome' = $cognome
    'Email' = "e2e.$stamp@test.local"
    'Telefono' = '1234567890'
    'Indirizzo' = 'Via Test 1'
    'Citta' = 'Milano'
    'StatoProvincia' = 'MI'
    'CodicePostale' = '20100'
    'Paese' = 'Italy'
    'Note' = 'Smoke test automation'
}
$null = Invoke-WebRequest -UseBasicParsing -Uri "$base/Clienti/Create" -Method Post -WebSession $session -Body $clientBody -ContentType 'application/x-www-form-urlencoded'

$clientSearch = Invoke-WebRequest -UseBasicParsing -Uri "$base/api/ClientiApi?search=$cognome" -WebSession $session
$clientResults = $clientSearch.Content | ConvertFrom-Json
if ($clientResults -isnot [System.Array]) {
    $clientResults = @($clientResults)
}

$client = $clientResults | Where-Object { $_.Cognome -eq $cognome } | Select-Object -First 1
if (-not $client) {
    throw 'Created client not found through API.'
}

$clientId = [int]$client.Id

$dashboard = Invoke-WebRequest -UseBasicParsing -Uri "$base/Measurements/Index?clienteId=$clientId" -WebSession $session
Assert-Contains $dashboard.Content 'GIACCA' 'Dashboard does not show Giacca.'
Assert-Contains $dashboard.Content 'CAMICIA' 'Dashboard does not show Camicia.'
Assert-Contains $dashboard.Content 'PANTALONE' 'Dashboard does not show Pantalone.'

function Get-DynamicCreateUrl([string]$html, [string]$label) {
    $escapedLabel = [regex]::Escape($label)
    $anchorPattern = '<a\s+[^>]*href="(?<href>[^"]*DynamicMeasurements/Create[^"]*)"[^>]*>(?<content>.*?)</a>'
    $matches = [regex]::Matches($html, $anchorPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline -bor [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $match = $matches | Where-Object { $_.Groups['content'].Value -match $escapedLabel } | Select-Object -First 1
    if (-not $match) {
        throw "Dynamic create URL not found for $label"
    }

    $href = [System.Net.WebUtility]::HtmlDecode($match.Groups['href'].Value)
    if ($href.StartsWith('/')) {
        return "$base$href"
    }

    return $href
}

$definitions = @(
    @{ Tipo = 'Giacca'; Values = @('46.5','102.0','91.0','64.0','73.0'); DetailsChecks = @('GIACCA','46.5') },
    @{ Tipo = 'Camicia'; Values = @('39.5','46.0','100.0','90.0','65.0','22.0','78.0'); DetailsChecks = @('CAMICIA','39.5') },
    @{ Tipo = 'Pantalone'; Values = @('90.0','101.0','29.0','82.0','18.0'); DetailsChecks = @('PANTALONE','90.0') }
)

$results = New-Object System.Collections.Generic.List[string]
foreach ($definition in $definitions) {
    $historyBeforeResponse = Invoke-WebRequest -UseBasicParsing -Uri "$base/api/ClientiApi/$clientId/misure" -WebSession $session
    $historyBefore = $historyBeforeResponse.Content | ConvertFrom-Json
    if ($historyBefore -isnot [System.Array]) {
        $historyBefore = @($historyBefore)
    }

    $maxHistoryIdBefore = 0
    if ($historyBefore.Count -gt 0) {
        $maxHistoryIdBefore = ($historyBefore | Measure-Object -Property Id -Maximum).Maximum
    }

    $createUrl = Get-DynamicCreateUrl -html $dashboard.Content -label $definition.Tipo
    $createPage = Invoke-WebRequest -UseBasicParsing -Uri $createUrl -WebSession $session
    Assert-Contains $createPage.Content $definition.Tipo.ToUpper() "Dynamic create page for $($definition.Tipo) not rendered."
    Assert-Contains $createPage.Content 'MISURA DINAMICA' "Dynamic engine not reached for $($definition.Tipo)."

    $hidden = Get-HiddenFields $createPage.Content
    $body = @{}
    foreach ($key in $hidden.Keys) {
        $body[$key] = $hidden[$key]
    }

    $valueFields = Get-ValueFields $createPage.Content
    if ($valueFields.Count -eq 0) {
        throw "No dynamic value fields found for $($definition.Tipo)."
    }

    for ($i = 0; $i -lt $valueFields.Count; $i++) {
        $fieldName = $valueFields[$i].Name
        $fieldType = $valueFields[$i].Value

        $value = if ($i -lt $definition.Values.Count) { $definition.Values[$i] } else { '1' }

        switch ($fieldType) {
            'checkbox' { $value = 'true' }
            'date' { $value = (Get-Date -Format 'yyyy-MM-dd') }
            'datetime-local' { $value = (Get-Date -Format 'yyyy-MM-ddTHH:mm') }
            'select' { if (-not $value) { $value = '1' } }
        }

        $body[$fieldName] = $value
    }

    $createPostResponse = Invoke-WebRequest -UseBasicParsing -Uri "$base/DynamicMeasurements/Create" -Method Post -WebSession $session -Body $body -ContentType 'application/x-www-form-urlencoded'
    $finalPath = $createPostResponse.BaseResponse.ResponseUri.AbsolutePath
    if ($finalPath -match '/DynamicMeasurements/Create') {
        $errors = New-Object System.Collections.Generic.List[string]

        foreach ($match in [regex]::Matches($createPostResponse.Content, '<div\s+[^>]*validation-summary-errors[^>]*>(?<msg>.*?)</div>', [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
            $text = Strip-Html $match.Groups['msg'].Value
            if (-not [string]::IsNullOrWhiteSpace($text)) {
                $errors.Add($text) | Out-Null
            }
        }

        foreach ($match in [regex]::Matches($createPostResponse.Content, '<span\s+[^>]*field-validation-error[^>]*>(?<msg>.*?)</span>', [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
            $text = Strip-Html $match.Groups['msg'].Value
            if (-not [string]::IsNullOrWhiteSpace($text)) {
                $errors.Add($text) | Out-Null
            }
        }

        foreach ($match in [regex]::Matches($createPostResponse.Content, '<li>(?<msg>.*?)</li>', [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
            $text = Strip-Html $match.Groups['msg'].Value
            if (-not [string]::IsNullOrWhiteSpace($text)) {
                $errors.Add($text) | Out-Null
            }
        }

        if ($errors.Count -eq 0) {
            throw "Dynamic create POST failed for $($definition.Tipo): Validation failed while posting dynamic measurement (no detailed error extracted)."
        }

        $joined = ($errors | Select-Object -Unique) -join ' | '
        throw "Dynamic create POST failed for $($definition.Tipo): $joined"
    }

    $historyResponse = Invoke-WebRequest -UseBasicParsing -Uri "$base/api/ClientiApi/$clientId/misure" -WebSession $session
    $history = $historyResponse.Content | ConvertFrom-Json
    if ($history -isnot [System.Array]) {
        $history = @($history)
    }

    $entry = $history | Where-Object { [int]$_.Id -gt [int]$maxHistoryIdBefore } | Sort-Object Id -Descending | Select-Object -First 1
    if (-not $entry) {
        throw "History entry missing for $($definition.Tipo)."
    }

    $detailUrl = "$base/Measurements/Details?id=$($entry.RecordId)&tipoMisura=$($definition.Tipo)&registryId=$($entry.Id)"
    $detailPage = Invoke-WebRequest -UseBasicParsing -Uri $detailUrl -WebSession $session
    foreach ($check in $definition.DetailsChecks) {
        Assert-Contains $detailPage.Content $check "Detail page for $($definition.Tipo) is missing '$check'."
    }

    $results.Add("$($definition.Tipo): OK (recordId=$($entry.RecordId), registryId=$($entry.Id))") | Out-Null
}

"SMOKE TEST OK`nClienteId: $clientId`n" + ($results -join "`n")