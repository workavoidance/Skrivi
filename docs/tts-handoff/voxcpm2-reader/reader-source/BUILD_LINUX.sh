#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"
DOTNET=${DOTNET:-/tmp/reader-dotnet/dotnet}
REFS=${REFS:-/tmp/reader-net48/build/.NETFramework/v4.8}
CSC=${CSC:-/tmp/reader-dotnet/sdk/8.0.419/Roslyn/bincore/csc.dll}
mkdir -p dist
refs=()
for name in mscorlib System System.Core System.Windows.Forms System.Drawing System.Net.Http System.Web.Extensions UIAutomationClient UIAutomationTypes WindowsBase; do
  refs+=("-r:$REFS/$name.dll")
done
"$DOTNET" "$CSC" -nologo -target:library -langversion:latest -out:dist/Reader.Core.dll "${refs[@]}" src/Reader.Core/*.cs
"$DOTNET" "$CSC" -nologo -target:winexe -platform:x64 -langversion:latest -win32manifest:src/Reader.Windows/app.manifest -out:dist/Reader.exe "${refs[@]}" -r:dist/Reader.Core.dll src/Reader.Windows/*.cs
cp src/Reader.Windows/App.config dist/Reader.exe.config
