#!/bin/bash
set -e
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
cd "$DIR"

echo "======================================================================="
echo "  Smart Anti-Sanction & Latency Router (Linux GUI)"
echo "======================================================================="

dotnet run --project src/SmartRouter.UI/SmartRouter.UI.csproj -c Release
