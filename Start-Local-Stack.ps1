<#
.SYNOPSIS
Starts the local Jolisoft API and Vite UI in separate PowerShell windows.

.DESCRIPTION
Run Build-Local-Backend.ps1 after C# changes. This script expects the already
built API DLL and avoids a second concurrent build at launch.
#>
param(
    [switch]$StopOnly,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [ValidateRange(1024, 65535)]
    [int]$ApiPort = 6041,
    [ValidateRange(1024, 65535)]
    [int]$UiPort = 6173
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiDirectory = Join-Path $projectRoot 'Jolisoft.Demo.WebAPI'
$uiDirectory = Join-Path $projectRoot 'ShowcaseShell'
$apiDll = Join-Path $apiDirectory "bin\$Configuration\net10.0\Jolisoft.Demo.WebAPI.dll"

function Get-ListeningProcessIds {
    param([int]$Port)

    return @(
        Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue |
            Select-Object -ExpandProperty OwningProcess -Unique
    )
}

function Stop-ProcessesById {
    param([int[]]$Ids, [string]$Label)

    foreach ($id in @($Ids | Where-Object { $_ } | Sort-Object -Unique)) {
        try {
            $process = Get-Process -Id $id -ErrorAction Stop
            Write-Host ("Stopping {0}: PID {1} ({2})" -f $Label, $id, $process.ProcessName)
            Stop-Process -Id $id -Force -ErrorAction Stop
        }
        catch {
            Write-Host ("Skipping {0}: PID {1} no longer exists." -f $Label, $id)
        }
    }
}

function Start-Terminal {
    param([string]$Title, [string]$WorkingDirectory, [string]$Command)

    $escapedDirectory = $WorkingDirectory.Replace("'", "''")
    $escapedTitle = $Title.Replace("'", "''")
    $fullCommand = @(
        "`$host.UI.RawUI.WindowTitle = '$escapedTitle'"
        "Set-Location -LiteralPath '$escapedDirectory'"
        $Command
    ) -join '; '

    Start-Process -FilePath powershell -ArgumentList @('-NoExit', '-Command', $fullCommand) -WorkingDirectory $WorkingDirectory
}

Stop-ProcessesById -Ids (Get-ListeningProcessIds -Port $ApiPort) -Label "JOLISOFT API port $ApiPort"
Stop-ProcessesById -Ids (Get-ListeningProcessIds -Port $UiPort) -Label "JOLISOFT UI port $UiPort"

if ($StopOnly) {
    Write-Host 'Cleanup complete. No new terminals were started because -StopOnly was supplied.'
    exit 0
}

if (-not (Test-Path -LiteralPath $apiDll)) {
    throw "Built API DLL was not found: $apiDll`nRun .\Build-Local-Backend.ps1 -Configuration $Configuration first."
}

if (-not (Test-Path -LiteralPath (Join-Path $uiDirectory 'node_modules'))) {
    throw "UI dependencies were not found in $uiDirectory`nRun npm install from the ui directory first."
}

$apiCommand = "`$env:ASPNETCORE_ENVIRONMENT = 'Development'; dotnet `"$apiDll`" --urls http://localhost:$ApiPort"
$uiCommand = "npm run dev -- --host localhost --port $UiPort --strictPort"

Start-Terminal -Title 'JOLISOFT API' -WorkingDirectory $apiDirectory -Command $apiCommand
Start-Terminal -Title 'JOLISOFT UI' -WorkingDirectory $uiDirectory -Command $uiCommand

Write-Host "Started Jolisoft local stack: API http://localhost:$ApiPort and UI http://localhost:$UiPort."
