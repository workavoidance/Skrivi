$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$source = Join-Path $root 'reader-source'
$output = Join-Path $root 'build'
$manifest = Get-Content (Join-Path $source 'MODEL_MANIFEST.json') -Raw | ConvertFrom-Json
New-Item -ItemType Directory -Force $output | Out-Null
# Developer SDK required. Runtime/model caches belonging to the app are untouched.
dotnet build (Join-Path $source 'src/Reader.Windows/Reader.Windows.csproj') -c Release -o $output
if ($LASTEXITCODE -ne 0) { throw 'Source build failed. A .NET 8 SDK and .NET Framework 4.8 reference assemblies are required.' }
$zip = Join-Path $output $manifest.windows_runtime.filename
if (!(Test-Path $zip) -or (Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant() -ne $manifest.windows_runtime.sha256) {
    Invoke-WebRequest $manifest.windows_runtime.url -OutFile ($zip + '.part')
    if ((Get-FileHash ($zip + '.part') -Algorithm SHA256).Hash.ToLowerInvariant() -ne $manifest.windows_runtime.sha256) { throw 'Runtime checksum mismatch' }
    Move-Item ($zip + '.part') $zip -Force
}
$unpack = Join-Path $output 'runtime-extracted'
Expand-Archive $zip $unpack -Force
$runtime = Join-Path $output 'runtime'
New-Item -ItemType Directory -Force $runtime | Out-Null
foreach ($name in @('crispasr.exe','openblas.dll','LICENSE','THIRD_PARTY_NOTICES.txt')) {
    $match = @(Get-ChildItem $unpack -Recurse -File | Where-Object Name -eq $name)
    if ($match.Count -ne 1) { throw "Expected exactly one runtime file: $name" }
    Copy-Item $match[0].FullName (Join-Path $runtime $name) -Force
}
Write-Host "Built $output\Reader.exe. Quit the old Reader before opening this one. Existing model storage will be reused."
