param([string]$ArtifactDirectory)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
if (-not $ArtifactDirectory) { $ArtifactDirectory = Join-Path $taskRoot 'work\live-engine-tests' }
New-Item -ItemType Directory -Path $ArtifactDirectory -Force | Out-Null
$taskRunDirectory = Join-Path $ArtifactDirectory ('run-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskRunDirectory -Force | Out-Null
$taskCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
# Keep the harness beside engine/ so archive resolution exercises the actual shipped layout.
$taskTestExe = Join-Path $taskRoot '.fb2kindle-live-tests.exe'
$taskSources = @('ArchiveBooks.cs', 'Fb2Converter.cs', 'KindleConverter.cs') | ForEach-Object { Join-Path (Join-Path $taskRoot 'source') $_ }
$taskSources += Get-ChildItem -LiteralPath (Join-Path $taskRoot 'source') -Filter 'L10n*.cs' | ForEach-Object { $_.FullName }
& $taskCompiler /nologo /target:exe /optimize+ /utf8output "/out:$taskTestExe" /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Xml.dll /r:System.Xml.Linq.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll @taskSources (Join-Path $PSScriptRoot 'LiveEngineTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Live engine test build failed' }
& $taskTestExe $taskRunDirectory
$taskExitCode = $LASTEXITCODE
Write-Output "ARTIFACTS $taskRunDirectory"
if ($taskExitCode -ne 0) { throw 'Live engine tests failed' }
