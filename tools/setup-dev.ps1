# VRSimulator - ishlab chiqish muhitini bir martalik sozlash (Windows)
# Bajaradi: Git LFS'ni yoqish, UnityYAMLMerge'ni ulash, kompyuterni tekshirish.
# Ishga tushirish: repo ildizidagi setup.bat faylini ikki marta bosing.

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo

function Step($text) { Write-Host "`n==> $text" -ForegroundColor Cyan }
function Ok($text)   { Write-Host "    [OK] $text" -ForegroundColor Green }
function Warn($text) { Write-Host "    [DIQQAT] $text" -ForegroundColor Yellow }

# 1. Git
Step 'Git tekshirilmoqda'
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Warn "Git topilmadi. https://git-scm.com dan o'rnating va setup.bat ni qayta ishga tushiring."
    exit 1
}
Ok (git --version)

# 2. Git LFS
Step 'Git LFS yoqilmoqda'
git lfs version *> $null
if ($LASTEXITCODE -ne 0) {
    Warn "Git LFS topilmadi. Git for Windows'ni qayta o'rnating (LFS komponenti belgilangan holda)."
} else {
    git lfs install | Out-Null
    git lfs pull
    Ok 'Git LFS yoqildi'
}

# 3. UnityYAMLMerge
Step 'UnityYAMLMerge ulanmoqda'
$editorRoot = Join-Path $env:ProgramFiles 'Unity\Hub\Editor'
$merge = Get-ChildItem $editorRoot -Directory -Filter '6000.*' -ErrorAction SilentlyContinue |
    Sort-Object { [version](($_.Name -replace '[^0-9.].*$', '')) } -Descending |
    ForEach-Object { Join-Path $_.FullName 'Editor\Data\Tools\UnityYAMLMerge.exe' } |
    Where-Object { Test-Path $_ } | Select-Object -First 1
if ($merge) {
    $mergeFwd = $merge -replace '\\', '/'
    git config merge.unityyamlmerge.name 'Unity SmartMerge'
    git config merge.unityyamlmerge.driver "'$mergeFwd' merge -p %O %B %A %A"
    git config merge.unityyamlmerge.recursive binary
    Ok "UnityYAMLMerge: $merge"
} else {
    Warn "Unity 6 LTS topilmadi. Unity o'rnatilgach, setup.bat ni qayta ishga tushiring."
}

# 4. Kompyuterni tekshirish
Step 'Kompyuter parametrlari tekshirilmoqda'
& (Join-Path $PSScriptRoot 'check-system.ps1')

$report = Join-Path $repo 'system-report.txt'
& (Join-Path $PSScriptRoot 'check-system.ps1') *> $report
Write-Host "`nHisobot saqlandi: $report" -ForegroundColor Cyan
Write-Host "Shu fayl mazmunini Claude'ga yuboring." -ForegroundColor Cyan
