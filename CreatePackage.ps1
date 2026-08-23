$ErrorActionPreference = "Stop"

$ProjectDir = $PSScriptRoot
$assemblyInfoPath = Join-Path $ProjectDir "Properties\AssemblyInfo.cs"
$assemblyInfo = Get-Content -Raw -LiteralPath $assemblyInfoPath
$versionMatch = [regex]::Match($assemblyInfo, 'AssemblyVersion\("([^"]+)"\)')

if (-not $versionMatch.Success) {
    throw "AssemblyVersion was not found in $assemblyInfoPath"
}

$sevenZipCommand = Get-Command "7z.exe" -ErrorAction SilentlyContinue
$sevenZipCandidates = @(
    if ($sevenZipCommand) { $sevenZipCommand.Source }
    Join-Path $env:ProgramFiles "7-Zip\7z.exe"
    if (${env:ProgramFiles(x86)}) { Join-Path ${env:ProgramFiles(x86)} "7-Zip\7z.exe" }
)
$sevenZip = $sevenZipCandidates |
    Where-Object { $_ -and (Test-Path -LiteralPath $_) } |
    Select-Object -First 1

if (-not $sevenZip) {
    throw "7-Zip was not found. Install 7-Zip or add 7z.exe to PATH."
}

$buildDir = Join-Path $ProjectDir "Build"
$sourceDir = Join-Path $buildDir "BepInEx"
$archivePath = Join-Path $buildDir ("Kat-FastSellInFlea-{0}.7z" -f $versionMatch.Groups[1].Value)

if (-not (Test-Path -LiteralPath $sourceDir)) {
    throw "Package source directory was not found: $sourceDir"
}

if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}

Push-Location $buildDir
try {
    & $sevenZip a -t7z -mx=9 $archivePath "BepInEx"
    if ($LASTEXITCODE -ne 0) {
        throw "7-Zip exited with code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}
