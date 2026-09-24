# VRSimulator - kompyuterni Unity 6 + Meta Quest (Android) ishlab chiqish uchun tekshirish
# Ishga tushirish (PowerShell):
#   powershell -ExecutionPolicy Bypass -File tools\check-system.ps1

$ErrorActionPreference = 'SilentlyContinue'
$results = @()
function Add-Result($name, $value, $ok, $note) {
    $script:results += [pscustomobject]@{ Parametr = $name; Qiymat = $value; Holat = $(if ($ok) { 'OK' } else { 'DIQQAT' }); Izoh = $note }
}

# OS
$os = Get-CimInstance Win32_OperatingSystem
$build = [int]$os.BuildNumber
Add-Result 'OS' "$($os.Caption) (build $build)" ($build -ge 19041) 'Windows 10 21H1+ yoki Windows 11 64-bit kerak'

# CPU
$cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
$cores = $cpu.NumberOfCores
Add-Result 'CPU' "$($cpu.Name.Trim()) - $cores yadro / $($cpu.NumberOfLogicalProcessors) oqim" ($cores -ge 4) 'Kamida 4 yadro, tavsiya 6-8+ (Android build va shader kompilyatsiyasi uchun)'

# RAM
$ramGB = [math]::Round($os.TotalVisibleMemorySize / 1MB, 1)
Add-Result 'RAM' "$ramGB GB" ($ramGB -ge 16) 'Minimal 8 GB, qulay ishlash uchun 16 GB+'

# GPU (VRAM registrdan olinadi - Win32_VideoController 4 GB dan ortig'ini noto'g'ri ko'rsatadi)
$gpuKeys = Get-ChildItem 'HKLM:\SYSTEM\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}' |
    Where-Object { $_.PSChildName -match '^\d{4}$' }
foreach ($k in $gpuKeys) {
    $p = Get-ItemProperty $k.PSPath
    if (-not $p.DriverDesc) { continue }
    $vram = $p.'HardwareInformation.qwMemorySize'
    $vramGB = if ($vram) { [math]::Round([double]$vram / 1GB, 1) } else { 0 }
    Add-Result 'GPU' "$($p.DriverDesc) - $vramGB GB VRAM, drayver $($p.DriverVersion)" ($vramGB -ge 4) 'DX11/DX12, 4 GB+ VRAM tavsiya (Quest Link uchun GTX 1060 6GB / RX 580 va yuqori)'
}

# Disklar
Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3' | ForEach-Object {
    $free = [math]::Round($_.FreeSpace / 1GB, 1); $size = [math]::Round($_.Size / 1GB, 1)
    Add-Result "Disk $($_.DeviceID)" "$free GB bo'sh / $size GB" ($free -ge 50) 'Unity + Android SDK/NDK + VS + loyiha uchun ~50 GB+ bo''sh joy'
}
Get-PhysicalDisk | ForEach-Object {
    Add-Result 'Fizik disk' "$($_.FriendlyName) ($($_.MediaType))" ($_.MediaType -ne 'HDD') 'SSD tavsiya etiladi'
}

# Dasturlar
$git = (git --version) 2>$null
Add-Result 'Git' $(if ($git) { $git } else { "o'rnatilmagan" }) ([bool]$git) 'https://git-scm.com'

$hubPath = "$env:ProgramFiles\Unity Hub\Unity Hub.exe"
Add-Result 'Unity Hub' $(if (Test-Path $hubPath) { $hubPath } else { 'topilmadi' }) (Test-Path $hubPath) 'https://unity.com/download'

$editorRoot = "$env:ProgramFiles\Unity\Hub\Editor"
$editors = @(Get-ChildItem $editorRoot -Directory -ErrorAction SilentlyContinue)
if ($editors.Count -eq 0) {
    Add-Result 'Unity Editor' 'topilmadi' $false 'Unity Hub orqali Unity 6 LTS (6000.x) o''rnating'
}
foreach ($e in $editors) {
    $android = Test-Path "$($e.FullName)\Editor\Data\PlaybackEngines\AndroidPlayer"
    $isU6 = $e.Name -like '6000.*'
    Add-Result "Unity $($e.Name)" $(if ($android) { 'Android Build Support bor' } else { 'Android Build Support YO''Q' }) ($isU6 -and $android) 'Unity 6 LTS + Android Build Support (OpenJDK, SDK & NDK) kerak'
}

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vs = if (Test-Path $vswhere) { & $vswhere -version '[17.0,18.0)' -property displayName } else { $null }
$vsUnity = if (Test-Path $vswhere) { & $vswhere -version '[17.0,18.0)' -requires Microsoft.VisualStudio.Workload.ManagedGame -property displayName } else { $null }
Add-Result 'Visual Studio 2022' $(if ($vs) { ($vs -join ', ') } else { 'topilmadi' }) ([bool]$vsUnity) '"Game development with Unity" workload kerak'

$metaLink = Test-Path "$env:ProgramFiles\Oculus\Support\oculus-runtime"
Add-Result 'Meta Quest Link' $(if ($metaLink) { "o'rnatilgan" } else { 'topilmadi' }) $true 'Ixtiyoriy: Play rejimida shlemni PC orqali sinash uchun'

$results | Format-Table -AutoSize -Wrap
$bad = @($results | Where-Object Holat -eq 'DIQQAT')
if ($bad.Count -eq 0) { Write-Host "`nNatija: kompyuter Unity 6 + Quest ishlab chiqish uchun tayyor." -ForegroundColor Green }
else { Write-Host "`nNatija: $($bad.Count) ta band e'tibor talab qiladi (DIQQAT qatorlari)." -ForegroundColor Yellow }
