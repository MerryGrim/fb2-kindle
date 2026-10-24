param([string]$ArtifactDirectory)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
if (-not $ArtifactDirectory) { $ArtifactDirectory = Join-Path $taskRoot 'work\ui-batch-tests' }
New-Item -ItemType Directory -Path $ArtifactDirectory -Force | Out-Null
$taskCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$taskExe = Join-Path $taskRoot 'BatchUiTests.exe'
& $taskCompiler /nologo /target:exe /utf8output "/out:$taskExe" "/r:$taskRoot\FB2Kindle.exe" /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'BatchUiTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'UI batch harness build failed' }
& $taskExe $ArtifactDirectory
if ($LASTEXITCODE -ne 0) { throw 'UI batch integration failed' }
