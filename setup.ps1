# Unity'yi kurduktan SONRA bir kez calistir.
# Kurulu Unity surumunu bulur ve ProjectVersion.txt'yi ona gore gunceller,
# boylece Unity Hub proje klasorunu sorunsuz acar.
#
# Calistirma:  powershell -ExecutionPolicy Bypass -File setup.ps1

$ErrorActionPreference = 'Stop'

Write-Host ''
Write-Host '  2D Platformer - kurulum yardimcisi' -ForegroundColor Cyan
Write-Host '  ----------------------------------' -ForegroundColor Cyan
Write-Host ''

# --- Unity kurulumlarini ara ---------------------------------------------
$searchRoots = @(
    "$env:ProgramFiles\Unity\Hub\Editor",
    "${env:ProgramFiles(x86)}\Unity\Hub\Editor",
    'C:\Unity\Hub\Editor',
    'D:\Unity\Hub\Editor'
)

# Unity Hub farkli bir klasore kurulmus olabilir - ayar dosyasindan oku
$hubConfig = Join-Path $env:APPDATA 'UnityHub\secondaryInstallPath.json'
if (Test-Path $hubConfig) {
    $customPath = (Get-Content $hubConfig -Raw).Trim('"', ' ', "`n", "`r")
    if ($customPath -and (Test-Path $customPath)) {
        $searchRoots += $customPath
    }
}

$editors = @()
foreach ($root in $searchRoots) {
    if (-not (Test-Path $root)) { continue }

    foreach ($dir in Get-ChildItem -Path $root -Directory -ErrorAction SilentlyContinue) {
        $exe = Join-Path $dir.FullName 'Editor\Unity.exe'
        if (Test-Path $exe) {
            $editors += [PSCustomObject]@{ Version = $dir.Name; Path = $exe }
        }
    }
}

if ($editors.Count -eq 0) {
    Write-Host '  Kurulu Unity bulunamadi.' -ForegroundColor Yellow
    Write-Host ''
    Write-Host '  1. https://unity.com/download adresinden Unity Hub indir'
    Write-Host '  2. Hub > Installs > Install Editor > Unity 6 LTS sec'
    Write-Host '  3. Kurulum bitince bu scripti tekrar calistir'
    Write-Host ''
    exit 1
}

# --- En yeni surumu sec ---------------------------------------------------
# Surum adlari "6000.0.58f1" gibi; sayisal parcalara ayirip siraliyoruz
$newest = $editors | Sort-Object -Property @{
    Expression = {
        $numbers = [regex]::Matches($_.Version, '\d+') | ForEach-Object { [int]$_.Value }
        # Ilk 4 sayiyi tek bir karsilastirilabilir degere cevir
        $value = 0
        for ($i = 0; $i -lt 4; $i++) {
            $part = if ($i -lt $numbers.Count) { $numbers[$i] } else { 0 }
            $value = $value * 100000 + $part
        }
        $value
    }
} -Descending | Select-Object -First 1

Write-Host '  Bulunan Unity surumleri:' -ForegroundColor Gray
foreach ($editor in $editors) {
    $marker = if ($editor.Version -eq $newest.Version) { '->' } else { '  ' }
    Write-Host "   $marker $($editor.Version)"
}
Write-Host ''

# --- ProjectVersion.txt'yi guncelle --------------------------------------
$versionFile = Join-Path $PSScriptRoot 'ProjectSettings\ProjectVersion.txt'
$versionDir = Split-Path $versionFile -Parent

if (-not (Test-Path $versionDir)) {
    New-Item -ItemType Directory -Path $versionDir -Force | Out-Null
}

"m_EditorVersion: $($newest.Version)" | Out-File -FilePath $versionFile -Encoding utf8 -NoNewline

Write-Host "  ProjectVersion.txt -> $($newest.Version) olarak ayarlandi." -ForegroundColor Green
Write-Host ''
Write-Host '  Siradaki adimlar:' -ForegroundColor Cyan
Write-Host '   1. Unity Hub > Add > Add project from disk'
Write-Host "   2. Bu klasoru sec:  $PSScriptRoot"
Write-Host '   3. Projeyi ac (ilk acilis birkac dakika surer)'
Write-Host '   4. Ust menuden:  Tools > 2D Platformer > Ornek Bolumu Olustur'
Write-Host '   5. Play tusuna bas'
Write-Host ''
