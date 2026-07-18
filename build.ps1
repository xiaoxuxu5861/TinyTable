$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$out = Join-Path $root "bin\Release"
$windows = $env:WINDIR
if ([string]::IsNullOrWhiteSpace($windows)) {
    $windows = $env:SystemRoot
}
if ([string]::IsNullOrWhiteSpace($windows)) {
    $windows = "C:\Windows"
}
$csc = Join-Path $windows "Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (-not (Test-Path $csc)) {
    $csc = Join-Path $windows "Microsoft.NET\Framework\v4.0.30319\csc.exe"
}
if (-not (Test-Path $csc)) {
    throw ".NET Framework C# compiler was not found."
}

New-Item -ItemType Directory -Force -Path $out | Out-Null
$sources = Get-ChildItem -Path (Join-Path $root "src"), (Join-Path $root "Properties") -Filter *.cs -Recurse |
    Select-Object -ExpandProperty FullName

& $csc /nologo /target:winexe /optimize+ /platform:anycpu `
    /out:"$out\TinyTable.exe" `
    /win32icon:"$root\assets\TinyTable.ico" `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Runtime.Serialization.dll `
    /reference:System.Windows.Forms.dll `
    $sources

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
}

Write-Host "Built: $out\TinyTable.exe"
