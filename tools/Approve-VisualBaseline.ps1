param(
    [Parameter(Mandatory = $true)]
    [string]$Scenario
)

$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot
$candidateDirectory = Join-Path `
    $projectPath `
    "Artifacts\VisualTests\$Scenario\captures"
$baselineDirectory = Join-Path `
    $projectPath `
    "Assets\Tests\Visual\Baselines\$Scenario"

if (-not (Test-Path -LiteralPath $candidateDirectory))
{
    throw "Não existem capturas candidatas para '$Scenario'."
}

New-Item -ItemType Directory -Force -Path $baselineDirectory | Out-Null
Get-ChildItem -LiteralPath $candidateDirectory -Filter "*.png" |
    Copy-Item -Destination $baselineDirectory -Force

Write-Host "Baseline visual aprovada para '$Scenario'."
