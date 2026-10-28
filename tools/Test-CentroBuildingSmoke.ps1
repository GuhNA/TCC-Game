param([string]$UnityPath = "D:\Unity\Editors\6000.3.11f1\Editor\Unity.exe")
$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot
$logDirectory = Join-Path $projectPath "Logs"
$resultPath = Join-Path $logDirectory "CentroBuildingSmoke.xml"
$logPath = Join-Path $logDirectory "CentroBuildingSmoke.log"
New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
if (Test-Path -LiteralPath $resultPath) { Remove-Item -LiteralPath $resultPath -Force }
$unityArguments = @("-batchmode", "-nographics", "-projectPath", $projectPath,
    "-runTests", "-testPlatform", "PlayMode", "-testFilter", "TCCGame.Tests.PlayMode.CentroBuildingSmokeTests",
    "-testResults", $resultPath, "-logFile", $logPath)
$process = Start-Process -FilePath $UnityPath -ArgumentList $unityArguments -WindowStyle Hidden -Wait -PassThru
if (-not (Test-Path -LiteralPath $resultPath)) { throw "Unity não gerou o resultado. Consulte $logPath." }
[xml]$results = Get-Content -Raw -LiteralPath $resultPath
$run = $results.'test-run'
if ([int]$run.failed -gt 0 -or [int]$run.passed -lt 1 -or $process.ExitCode -ne 0) {
    throw "Verificação do Centro falhou. Consulte $resultPath e $logPath."
}
Write-Host "Centro: $($run.passed)/$($run.total) teste funcional passou."
