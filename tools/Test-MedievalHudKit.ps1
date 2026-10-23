param()

$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot
$legacyFolder = Join-Path $projectPath "Assets\Resources\UI\RecruitmentStyle"
$sharedFolder = Join-Path $projectPath "Assets\Resources\UI\Theme\MedievalPixel"
$themePath = Join-Path $projectPath "Assets\Resources\UI\Theme\MedievalPixelHudTheme.asset"
$prefabPath = Join-Path $projectPath "Assets\Resources\UI\RecruitmentCanvas.prefab"

$assetNames = @(
    "WoodPanel", "ParchmentPanel", "MetalPanel",
    "GoldButton", "BronzeButton", "RedButton",
    "WarriorBanner", "ArcherBanner", "MageBanner",
    "CoinIcon", "HourglassIcon", "CheckIcon", "HelmetIcon", "HeaderDrape"
)

foreach ($assetName in $assetNames)
{
    $legacyPath = Join-Path $legacyFolder "$assetName.png"
    $sharedPath = Join-Path $sharedFolder "$assetName.png"
    if (-not (Test-Path -LiteralPath $legacyPath) -or
        -not (Test-Path -LiteralPath $sharedPath))
    {
        throw "Asset ausente no kit: $assetName"
    }

    $legacyHash = (Get-FileHash -LiteralPath $legacyPath -Algorithm SHA256).Hash
    $sharedHash = (Get-FileHash -LiteralPath $sharedPath -Algorithm SHA256).Hash
    if ($legacyHash -ne $sharedHash)
    {
        throw "O asset compartilhado divergiu do baseline: $assetName"
    }
}

$theme = Get-Content -Raw -LiteralPath $themePath
$requiredThemeFields = @(
    "font", "woodPanel", "parchmentPanel", "metalPanel",
    "goldButton", "bronzeButton", "redButton", "warriorBanner",
    "archerBanner", "mageBanner", "coinIcon", "hourglassIcon",
    "checkIcon", "helmetIcon", "headerDrape"
)
foreach ($field in $requiredThemeFields)
{
    if ($theme -notmatch "(?m)^  $field`: \{fileID: (?!0)")
    {
        throw "Referência ausente no tema compartilhado: $field"
    }
}

$prefab = Get-Content -Raw -LiteralPath $prefabPath
foreach ($assetName in $assetNames)
{
    $metaPath = Join-Path $sharedFolder "$assetName.png.meta"
    $guidLine = Select-String -LiteralPath $metaPath -Pattern '^guid:'
    $guid = $guidLine.Line.Split(':')[1].Trim()
    if (-not $prefab.Contains($guid))
    {
        throw "O prefab não está consumindo o asset compartilhado: $assetName"
    }
}

Write-Host "Kit medieval aprovado: 14 sprites idênticos ao baseline, tema completo e prefab migrado."
