$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
dotnet run --project tests/Reader.Core.Tests.csproj -- Norwegian-Test.wav
if ($LASTEXITCODE -ne 0) { throw 'Core checks failed' }
