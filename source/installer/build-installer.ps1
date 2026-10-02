$ErrorActionPreference = 'Stop'
$taskInnoCompiler = Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Source
if (-not $taskInnoCompiler) {
    foreach ($taskCandidate in @("$env:ProgramFiles\Inno Setup 7\ISCC.exe", "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe")) {
        if (Test-Path -LiteralPath $taskCandidate) { $taskInnoCompiler = $taskCandidate; break }
    }
}
if (-not $taskInnoCompiler) { throw 'Install Inno Setup 7.1 or newer from https://jrsoftware.org/isdl.php and put ISCC.exe on PATH.' }
& (Join-Path $PSScriptRoot '..\build.ps1')
& $taskInnoCompiler '/Qp' (Join-Path $PSScriptRoot 'FB2Kindle.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer build failed' }
