param(
    [switch]$RequireInstaller
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$dist = Join-Path $root "dist"
$stageRoot = Join-Path $root "build\packages"
$portableStage = Join-Path $stageRoot "TinyTable-Portable"
$exe = Join-Path $root "bin\Release\TinyTable.exe"
$marker = Join-Path $root "packaging\TinyTable.local-data"
$portableMarker = Join-Path $root "packaging\TinyTable.portable"
$portableReadme = Join-Path $root "packaging\PORTABLE-README.txt"

& (Join-Path $root "build.ps1")

$versionInfo = [Diagnostics.FileVersionInfo]::GetVersionInfo($exe)
$version = $versionInfo.FileVersion
if ([string]::IsNullOrWhiteSpace($version)) {
    $version = "0.1.0"
}
$version = ($version -split "\.")[0..2] -join "."

New-Item -ItemType Directory -Force -Path $dist | Out-Null
New-Item -ItemType Directory -Force -Path $stageRoot | Out-Null

$resolvedRoot = [IO.Path]::GetFullPath($root).TrimEnd('\') + '\'
$resolvedStage = [IO.Path]::GetFullPath($portableStage)
if (-not $resolvedStage.StartsWith($resolvedRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Unsafe portable staging path: $resolvedStage"
}
if (Test-Path -LiteralPath $portableStage) {
    Remove-Item -LiteralPath $portableStage -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $portableStage | Out-Null

Copy-Item -LiteralPath $exe -Destination $portableStage
Copy-Item -LiteralPath $marker -Destination $portableStage
Copy-Item -LiteralPath $portableMarker -Destination $portableStage
Copy-Item -LiteralPath $portableReadme -Destination $portableStage
Copy-Item -LiteralPath (Join-Path $root "README.md") -Destination $portableStage

$portableZip = Join-Path $dist ("TinyTable-Portable-" + $version + ".zip")
if (Test-Path -LiteralPath $portableZip) {
    Remove-Item -LiteralPath $portableZip -Force
}
Compress-Archive -Path $portableStage -DestinationPath $portableZip -CompressionLevel Optimal
Write-Host "Portable: $portableZip"

$compilerCandidates = @(
    (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 7\ISCC.exe"),
    (Join-Path $env:ProgramFiles "Inno Setup 7\ISCC.exe"),
    (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
    (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe"),
    (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 7\ISCC.exe"),
    (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe")
) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }

$iscc = $compilerCandidates | Select-Object -First 1
if (-not $iscc) {
    $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($command) { $iscc = $command.Source }
}
if (-not $iscc) {
    $uninstallRoots = @(
        "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
        "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
        "HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*"
    )
    foreach ($uninstallRoot in $uninstallRoots) {
        $installation = Get-ItemProperty $uninstallRoot -ErrorAction SilentlyContinue |
            Where-Object { $_.DisplayName -like "Inno Setup version*" } |
            Sort-Object DisplayVersion -Descending |
            Select-Object -First 1
        if ($installation -and $installation.InstallLocation) {
            $candidate = Join-Path $installation.InstallLocation "ISCC.exe"
            if (Test-Path -LiteralPath $candidate) {
                $iscc = $candidate
                break
            }
        }
    }
}

if ($iscc) {
    & $iscc ("/DMyAppVersion=" + $version) (Join-Path $root "packaging\TinyTable.iss")
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup failed with exit code $LASTEXITCODE"
    }
    Write-Host "Installer: $(Join-Path $dist ("TinyTable-Setup-" + $version + ".exe"))"
}
else {
    $message = "Inno Setup compiler was not found. Portable ZIP was built; install Inno Setup and run build-packages.ps1 again for Setup.exe."
    if ($RequireInstaller) { throw $message }
    Write-Warning $message
}
