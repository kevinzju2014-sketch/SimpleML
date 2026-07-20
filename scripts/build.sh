#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"
cd "$ROOT/GHA_Project"
dotnet build SimpleML.csproj -c Release -p:UseNuGetRhino=true -p:RhinoMajorVersion=7
echo "GHA: $ROOT/GHA_Project/bin/Release/SimpleML.gha"
