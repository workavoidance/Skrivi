$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
# Requires a .NET 8 SDK on the developer machine only. End users do not need it.
dotnet build src/Reader.Windows/Reader.Windows.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Copy-Item src/Reader.Windows/bin/Release/net48/Reader.exe ../Reader.exe -Force
Copy-Item src/Reader.Windows/bin/Release/net48/Reader.Core.dll ../Reader.Core.dll -Force
Copy-Item src/Reader.Windows/bin/Release/net48/Reader.exe.config ../Reader.exe.config -Force
Write-Host 'Built ../Reader.exe. Keep the provided runtime folder beside it.'
