param([string]$ArtifactDirectory)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
if (-not $ArtifactDirectory) { $ArtifactDirectory = Join-Path $taskRoot 'work\conversion-tests' }
New-Item -ItemType Directory -Path $ArtifactDirectory -Force | Out-Null
$taskCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$taskTestExe = Join-Path $ArtifactDirectory 'ConversionProgressTests.exe'
$taskSources = @('ArchiveBooks.cs', 'Fb2Converter.cs', 'KindleConverter.cs') | ForEach-Object { Join-Path (Join-Path $taskRoot 'source') $_ }
$taskSources += Get-ChildItem -LiteralPath (Join-Path $taskRoot 'source') -Filter 'L10n*.cs' | ForEach-Object { $_.FullName }
& $taskCompiler /nologo /target:exe /optimize+ /utf8output "/out:$taskTestExe" /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Xml.dll /r:System.Xml.Linq.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll @taskSources (Join-Path $PSScriptRoot 'ConversionProgressTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Conversion test build failed' }
& $taskTestExe $ArtifactDirectory
if ($LASTEXITCODE -ne 0) { throw 'Conversion tests failed' }
