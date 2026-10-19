$ErrorActionPreference = 'Stop'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path $csc)) { throw 'Compilateur .NET Framework introuvable.' }
$source = Join-Path $PSScriptRoot 'MinecraftSync.cs'
$output = Join-Path $PSScriptRoot 'MinecraftSync.exe'
& $csc /nologo /target:winexe "/out:$output" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll $source
if ($LASTEXITCODE -ne 0) { throw 'Compilation impossible.' }
Write-Output "Executable : $output"
