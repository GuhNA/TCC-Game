param(
    [string]$Scenario,
    [string]$UnityPath = "D:\Unity\Editors\6000.3.11f1\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot
$logDirectory = Join-Path $projectPath "Logs"
$logPath = Join-Path $logDirectory "VisualReview.log"
$arguments = @(
    "-batchmode",
    "-projectPath", $projectPath,
    "-executeMethod", "TCCGame.Tests.Visual.VisualReviewRunner.RunFromCommandLine",
    "-logFile", $logPath
)

if (-not [string]::IsNullOrWhiteSpace($Scenario))
{
    $arguments += @("-visualReviewScenario", $Scenario)
}

New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
$process = Start-Process `
    -FilePath $UnityPath `
    -ArgumentList $arguments `
    -Wait `
    -PassThru `
    -WindowStyle Hidden

if ($process.ExitCode -ne 0)
{
    throw "A revisão visual falhou. Consulte $logPath."
}

$scenarioDirectory = if ([string]::IsNullOrWhiteSpace($Scenario))
{
    Join-Path $projectPath "Artifacts\VisualTests"
}
else
{
    Join-Path $projectPath "Artifacts\VisualTests\$Scenario"
}

Write-Host "Revisão visual concluída. Artefatos: $scenarioDirectory"
