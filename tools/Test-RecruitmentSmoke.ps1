param(
    [string]$UnityPath = "D:\Unity\Editors\6000.3.11f1\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot
$logDirectory = Join-Path $projectPath "Logs"
$resultPath = Join-Path $logDirectory "RecruitmentSmoke.xml"
$logPath = Join-Path $logDirectory "RecruitmentSmoke.log"

New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null

if (Test-Path -LiteralPath $resultPath)
{
    Remove-Item -LiteralPath $resultPath -Force
}

$unityArguments = @(
    "-batchmode",
    "-nographics",
    "-projectPath", $projectPath,
    "-runTests",
    "-testPlatform", "PlayMode",
    "-testFilter", "TCCGame.Tests.PlayMode.RecruitmentSmokeTests",
    "-testResults", $resultPath,
    "-logFile", $logPath
)

$process = Start-Process `
    -FilePath $UnityPath `
    -ArgumentList $unityArguments `
    -Wait `
    -PassThru `
    -WindowStyle Hidden

if (-not (Test-Path -LiteralPath $resultPath))
{
    throw "Unity não gerou o resultado. Consulte $logPath."
}

[xml]$results = Get-Content -Raw -LiteralPath $resultPath
$run = $results.'test-run'
$passed = [int]$run.passed
$failed = [int]$run.failed

if ($failed -gt 0)
{
    Write-Host "Teste de recrutamento falhou: $passed passou, $failed falhou."
    Write-Host "Relatório: $resultPath"
    exit 1
}

Write-Host "Teste de recrutamento aprovado: $passed de $($run.total)."
