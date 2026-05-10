param(
    [string]$ConnectionString,
    [string]$OutputRoot = "src/Migrations/History/current"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    $scriptDir = Split-Path -Parent $PSCommandPath
    return (Resolve-Path (Join-Path $scriptDir "..")).Path
}

function Get-ConnectionString([string]$repoRoot, [string]$override) {
    if ($override) { return $override }

    $appSettings = Join-Path $repoRoot "src/Migrations/appsettings.json"
    if (!(Test-Path $appSettings)) {
        throw "Cannot find appsettings at $appSettings"
    }

    $json = Get-Content $appSettings -Raw | ConvertFrom-Json
    $fromJson = $json.ConnectionStrings.ChessConnection
    if ([string]::IsNullOrWhiteSpace($fromJson)) {
        throw "ConnectionStrings:ChessConnection is missing in src/Migrations/appsettings.json"
    }

    return $fromJson
}

function Ensure-SqlServerModule {
    if (!(Get-Module -ListAvailable -Name SqlServer)) {
        throw "PowerShell module 'SqlServer' is required. Install once with: Install-Module SqlServer -Scope CurrentUser"
    }

    Import-Module SqlServer -ErrorAction Stop
}

function Normalize-Connection([string]$cs) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $cs
    return [PSCustomObject]@{
        Server = $builder.DataSource
        Database = $builder.InitialCatalog
        LoginSecure = $builder.IntegratedSecurity
        User = $builder.UserID
        Password = $builder.Password
        Full = $builder.ConnectionString
    }
}

function Clear-Directory([string]$path) {
    if (Test-Path $path) {
        Get-ChildItem $path -Force | ForEach-Object {
            if ($_.Name -ne ".gitkeep") {
                Remove-Item $_.FullName -Recurse -Force
            }
        }
    } else {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}

function Safe-FileName([string]$schema, [string]$name) {
    $value = "$schema.$name"
    $invalid = [System.IO.Path]::GetInvalidFileNameChars() -join ""
    $pattern = "[{0}]" -f [Regex]::Escape($invalid)
    return ([Regex]::Replace($value, $pattern, "_")) + ".sql"
}

function Script-Objects([Microsoft.SqlServer.Management.Smo.Database]$db, [string]$outputRoot) {
    $scripter = New-Object Microsoft.SqlServer.Management.Smo.Scripter ($db.Parent)
    $scripter.Options.AnsiFile = $true
    $scripter.Options.IncludeHeaders = $true
    $scripter.Options.ScriptDrops = $false
    $scripter.Options.WithDependencies = $false
    $scripter.Options.SchemaQualify = $true
    $scripter.Options.DriAll = $true
    $scripter.Options.Indexes = $true
    $scripter.Options.Triggers = $true
    $scripter.Options.NoCollation = $false

    $groups = @(
        @{ Name = "tables"; Items = @($db.Tables | Where-Object { -not $_.IsSystemObject }) },
        @{ Name = "views"; Items = @($db.Views | Where-Object { -not $_.IsSystemObject }) },
        @{ Name = "procedures"; Items = @($db.StoredProcedures | Where-Object { -not $_.IsSystemObject }) },
        @{ Name = "functions"; Items = @($db.UserDefinedFunctions | Where-Object { -not $_.IsSystemObject }) },
        @{ Name = "types"; Items = @($db.UserDefinedTableTypes | Where-Object { -not $_.IsSystemObject }) }
    )

    foreach ($group in $groups) {
        $groupPath = Join-Path $outputRoot $group.Name
        New-Item -ItemType Directory -Path $groupPath -Force | Out-Null

        $sorted = $group.Items | Sort-Object Schema, Name
        foreach ($obj in $sorted) {
            $scriptParts = $scripter.Script($obj.Urn)
            if ($null -eq $scriptParts -or $scriptParts.Count -eq 0) {
                continue
            }

            $body = ($scriptParts -join [Environment]::NewLine).Trim() + [Environment]::NewLine
            $fileName = Safe-FileName $obj.Schema $obj.Name
            $filePath = Join-Path $groupPath $fileName
            Set-Content -Path $filePath -Value $body -Encoding UTF8
        }
    }
}

$repoRoot = Get-RepoRoot
$outputPath = Join-Path $repoRoot $OutputRoot
Clear-Directory $outputPath
Ensure-SqlServerModule

$conn = Normalize-Connection (Get-ConnectionString -repoRoot $repoRoot -override $ConnectionString)
$server = New-Object Microsoft.SqlServer.Management.Smo.Server $conn.Server
if ($conn.LoginSecure) {
    $server.ConnectionContext.LoginSecure = $true
} elseif (-not [string]::IsNullOrWhiteSpace($conn.User)) {
    $server.ConnectionContext.LoginSecure = $false
    $server.ConnectionContext.set_Login($conn.User)
    $server.ConnectionContext.set_SecurePassword((ConvertTo-SecureString $conn.Password -AsPlainText -Force))
}

$db = $server.Databases[$conn.Database]
if ($null -eq $db) {
    throw "Database '$($conn.Database)' not found on server '$($conn.Server)'."
}

Script-Objects -db $db -outputRoot $outputPath

Write-Host "Exported schema history to $outputPath" -ForegroundColor Green
