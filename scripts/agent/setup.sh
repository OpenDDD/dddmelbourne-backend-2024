#!/usr/bin/env bash
# Installs .NET 6 SDK, Azurite and Azure Functions Core Tools into ./.local-tools (idempotent, no sudo).
set -euo pipefail
source "$(dirname "$0")/env.sh"

if [[ ! -x "$TOOLS/dotnet/dotnet" ]]; then
  mkdir -p "$TOOLS"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$TOOLS/dotnet-install.sh"
  bash "$TOOLS/dotnet-install.sh" --channel 6.0 --install-dir "$TOOLS/dotnet"
fi

AZURITE_VERSION=3.35.0
# Newer Core Tools releases break on Node 18 (ESM require in install.js).
FUNC_VERSION=4.0.6280

installed_version() {
  grep -m1 '"version"' "$TOOLS/npm/lib/node_modules/$1/package.json" 2>/dev/null | sed -E 's/.*: *"([^"]+)".*/\1/'
}

if [[ "$(installed_version azurite)" != "$AZURITE_VERSION" ]]; then
  npm install -g --prefix "$TOOLS/npm" "azurite@$AZURITE_VERSION"
fi

if [[ "$(installed_version azure-functions-core-tools)" != "$FUNC_VERSION" ]]; then
  npm install -g --prefix "$TOOLS/npm" "azure-functions-core-tools@$FUNC_VERSION" --unsafe-perm true
fi
chmod +x "$TOOLS/npm/lib/node_modules/azure-functions-core-tools/lib/main.js"

dotnet --version
azurite --version
func --version
