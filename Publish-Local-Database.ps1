<#
.SYNOPSIS
Builds the Jolisoft database DACPAC and generates a reviewable SQLPackage deployment script.

.DESCRIPTION
The default action is safe: it only builds the SDK-style SQL project and writes the
SQLPackage deployment plan under artifacts\database. Supply -Publish to apply that
same DACPAC after the deployment plan has been generated successfully.

Provide the connection string with -ConnectionString, set
ConnectionStrings__DefaultConnection for the current PowerShell process, or create
the private, untracked file secrets\jolisoft-demo.connection-string.txt. That file
must contain only the connection string.
#>
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [string]$ConnectionString,
    [string]$ConnectionStringFile,
    [switch]$Publish
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$databaseProject = Join-Path $projectRoot 'Jolisoft.Demo.Database\Jolisoft.Demo.Database.sqlproj'
$dacpac = Join-Path $projectRoot "Jolisoft.Demo.Database\bin\$Configuration\Jolisoft.Demo.Database.dacpac"
$artifactDirectory = Join-Path $projectRoot 'artifacts\database'

if (-not $ConnectionStringFile) {
    $ConnectionStringFile = Join-Path $projectRoot 'secrets\jolisoft-demo.connection-string.txt'
}

if (-not $ConnectionString) {
    $ConnectionString = $env:ConnectionStrings__DefaultConnection
}

if (-not $ConnectionString -and (Test-Path -LiteralPath $ConnectionStringFile -PathType Leaf)) {
    $ConnectionString = (Get-Content -LiteralPath $ConnectionStringFile -Raw).Trim()
}

if (-not $ConnectionString) {
    throw "No database connection string was supplied. Use -ConnectionString, set ConnectionStrings__DefaultConnection, or create the private file $ConnectionStringFile."
}

$sqlPackageCommand = Get-Command sqlpackage.exe -ErrorAction SilentlyContinue
if (-not $sqlPackageCommand) {
    $sqlPackageCommand = Get-Command sqlpackage -ErrorAction SilentlyContinue
}

if (-not $sqlPackageCommand) {
    throw 'SQLPackage was not found on PATH. Install the Microsoft.SqlPackage dotnet tool, then run this script again.'
}

dotnet build $databaseProject --configuration $Configuration --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    throw 'The database project build failed. SQLPackage was not run, so no stale DACPAC can be deployed.'
}

if (-not (Test-Path -LiteralPath $dacpac -PathType Leaf)) {
    throw "The expected DACPAC was not produced: $dacpac"
}

New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$scriptPath = Join-Path $artifactDirectory "JolisoftDemoDB-$timestamp.sql"
$commonArguments = @(
    "/SourceFile:$dacpac",
    "/TargetConnectionString:$ConnectionString",
    '/p:BlockOnPossibleDataLoss=True',
    '/p:DropObjectsNotInSource=False'
)

& $sqlPackageCommand.Path '/Action:Script' @commonArguments "/OutputPath:$scriptPath"
if ($LASTEXITCODE -ne 0) {
    throw 'SQLPackage could not generate the deployment script. Nothing was published.'
}

Write-Host "Deployment script generated: $scriptPath"

if (-not $Publish) {
    Write-Host 'Review the generated script. Re-run with -Publish to apply this freshly built DACPAC.'
    exit 0
}

& $sqlPackageCommand.Path '/Action:Publish' @commonArguments
if ($LASTEXITCODE -ne 0) {
    throw 'SQLPackage publish failed. Review the generated deployment script and SQLPackage output.'
}

Write-Host 'SQLPackage publish completed successfully.'
