param(
    [string]$AppSource = (Join-Path $PSScriptRoot '..\..'),
    [string]$ReleaseDir = (Join-Path $PSScriptRoot '..\..\..'),
    [string]$InnoCompiler = '',
    [ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '1.3.0',
    [string]$VisualStudioPath = '',
    [switch]$SkipApplicationBuild
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$taskSource = (Resolve-Path -LiteralPath $AppSource).Path
$taskRelease = [IO.Path]::GetFullPath($ReleaseDir)
if ([string]::IsNullOrWhiteSpace($InnoCompiler)) {
    $taskInnoCommand = Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($taskInnoCommand) { $InnoCompiler = $taskInnoCommand.Source }
}
if ([string]::IsNullOrWhiteSpace($InnoCompiler)) {
    foreach ($taskCandidate in @("$env:ProgramFiles\Inno Setup 7\ISCC.exe", "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe", 'D:\AI\Tools\InnoSetup7\ISCC.exe')) {
        if (Test-Path -LiteralPath $taskCandidate) { $InnoCompiler = $taskCandidate; break }
    }
}
if ([string]::IsNullOrWhiteSpace($InnoCompiler) -or -not (Test-Path -LiteralPath $InnoCompiler)) { throw 'Install Inno Setup 7.1 or newer, or pass -InnoCompiler.' }
$taskCompilerVersion = (& $InnoCompiler '--version' | Select-Object -Last 1)
if ($taskCompilerVersion -notmatch '^([0-9]+)\.([0-9]+)') { throw 'Could not determine the Inno Setup compiler version.' }
if ([int]$Matches[1] -lt 7 -or ([int]$Matches[1] -eq 7 -and [int]$Matches[2] -lt 1)) { throw 'Inno Setup 7.1 or newer is required for Mira Mint dynamic themes.' }
if (-not $SkipApplicationBuild) { & (Join-Path $taskSource 'source\build.ps1') }
foreach ($taskRelative in @('FB2Kindle.exe','FB2Kindle.exe.config','README.md','LICENSE','THIRD-PARTY-NOTICES.md','engine\Calibre\ebook-convert.exe','engine\Calibre\LICENSE','engine\Calibre\COPYRIGHT','engine\Calibre\calibre-9.15.0-source.tar.xz','engine\Archive7z\7z.exe','engine\Archive7z\7z.dll','engine\Archive7z\LICENSE.txt','engine\Archive7z\COPYING.txt','engine\Archive7z\UNRAR-LICENSE.txt','engine\Archive7z\7z2600-src.tar.xz','engine\InnoSetup\LICENSE.txt')) {
    if (-not (Test-Path -LiteralPath (Join-Path $taskSource $taskRelative) -PathType Leaf)) { throw "The complete offline installer requires: $taskRelative" }
}
$taskTarget = Join-Path $taskRelease "FB2Kindle-$Version-Setup.exe"
if (Test-Path -LiteralPath $taskTarget) { throw "Installer output already exists: $taskTarget. Use a new release directory." }
New-Item -ItemType Directory -Path $taskRelease -Force | Out-Null
$taskStage = Join-Path $taskRelease 'installer-build'
New-Item -ItemType Directory -Path $taskStage -Force | Out-Null
& $InnoCompiler '/Qp' "/DAppSource=$taskSource" "/DReleaseDir=$taskStage" "/DAppVersion=$Version" '/DOutputName=FB2Kindle.Inner' (Join-Path $PSScriptRoot 'FB2Kindle.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer build failed' }
$taskBootstrap = @{
    InnerSetup = (Join-Path $taskStage 'FB2Kindle.Inner.exe')
    OutputExe = $taskTarget
    Version = $Version
}
if (-not [string]::IsNullOrWhiteSpace($VisualStudioPath)) { $taskBootstrap.VisualStudioPath = $VisualStudioPath }
& (Join-Path $PSScriptRoot 'build-bootstrap.ps1') @taskBootstrap
if (-not (Test-Path -LiteralPath $taskTarget)) { throw 'The final installer was not created.' }
Write-Output $taskTarget
