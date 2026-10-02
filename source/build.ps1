$ErrorActionPreference = 'Stop'
$taskCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $taskCompiler)) { $taskCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$taskOutput = Join-Path $PSScriptRoot '..\FB2Kindle.exe'
$taskSources = Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | ForEach-Object { $_.FullName }
& $taskCompiler /nologo /target:winexe /platform:anycpu /optimize+ /utf8output "/out:$taskOutput" "/win32manifest:$PSScriptRoot\app.manifest" "/win32icon:$PSScriptRoot\app.ico" /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Xml.dll /r:System.Xml.Linq.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll @taskSources
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Write-Output $taskOutput
