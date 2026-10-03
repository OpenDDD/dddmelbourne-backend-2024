#!/usr/bin/env bash
# Installs .NET 6 SDK, Azurite and Azure Functions Core Tools into ./.local-tools (idempotent, no sudo).
set -euo pipefail
source "$(dirname "$0")/env.sh"

if [[ ! -x "$TOOLS/dotnet/dotnet" ]]; then
  mkdir -p "$TOOLS"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$TOOLS/dotnet-install.sh"
  bash "$TOOLS/dotnet-install.sh" --channel 6.0 --install-dir "$TOOLS/dotnet"
fi

if [[ ! -x "$TOOLS/npm/bin/azurite" ]]; then
  npm install -g --prefix "$TOOLS/npm" azurite
fi

if [[ ! -x "$TOOLS/npm/bin/func" ]]; then
  # Newer releases break on Node 18 (ESM require in install.js).
  npm install -g --prefix "$TOOLS/npm" azure-functions-core-tools@4.0.6280 --unsafe-perm true
fi
chmod +x "$TOOLS/npm/lib/node_modules/azure-functions-core-tools/lib/main.js"

dotnet --version
azurite --version
func --version
