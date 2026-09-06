<#
.SYNOPSIS
Builds the Jolisoft controller API for local use.
#>
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $projectRoot 'Jolisoft.Prototypes.slnx'

if (-not (Test-Path -LiteralPath $solution)) {
    throw "Jolisoft solution was not found: $solution"
}

dotnet build $solution --configuration $Configuration --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    throw 'Build failed for Jolisoft.'
}
