param([string]$ArtifactDirectory)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
if (-not $ArtifactDirectory) { $ArtifactDirectory = Join-Path $taskRoot 'work\window-placement-tests' }
$ArtifactDirectory = [IO.Path]::GetFullPath($ArtifactDirectory)
New-Item -ItemType Directory -Path $ArtifactDirectory -Force | Out-Null
$taskCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$taskExe = Join-Path $ArtifactDirectory 'WindowPlacementTests.exe'
$taskSources = Get-ChildItem -LiteralPath (Join-Path $taskRoot 'source') -Filter '*.cs' | ForEach-Object { $_.FullName }
& $taskCompiler /nologo /target:exe /main:WindowPlacementTests /optimize+ /utf8output "/out:$taskExe" "/win32manifest:$taskRoot\source\app.manifest" /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Xml.dll /r:System.Xml.Linq.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll @taskSources (Join-Path $PSScriptRoot 'WindowPlacementTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Window placement test build failed' }
& $taskExe $ArtifactDirectory
if ($LASTEXITCODE -ne 0) { throw 'Window placement tests failed' }
