#!/usr/bin/env bash
# Installs .NET 10 SDK, Node LTS, Azurite and Azure Functions Core Tools into ./.local-tools (idempotent, no sudo).
set -euo pipefail
source "$(dirname "$0")/env.sh"

DOTNET_CHANNEL=10.0
NODE_VERSION=24.21.0
AZURITE_VERSION=3.35.0
# Core Tools releases that support .NET 10 isolated need Node >= 22, so setup installs a local Node.
FUNC_VERSION=4.15.2

mkdir -p "$TOOLS"

if ! "$TOOLS/dotnet/dotnet" --list-sdks 2>/dev/null | grep -q "^${DOTNET_CHANNEL%.0}\."; then
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$TOOLS/dotnet-install.sh"
  bash "$TOOLS/dotnet-install.sh" --channel "$DOTNET_CHANNEL" --install-dir "$TOOLS/dotnet"
fi

if [[ "$("$TOOLS/node/bin/node" --version 2>/dev/null)" != "v$NODE_VERSION" ]]; then
  case "$(uname -s)-$(uname -m)" in
    Linux-x86_64) node_platform=linux-x64 ;;
    Linux-aarch64) node_platform=linux-arm64 ;;
    Darwin-x86_64) node_platform=darwin-x64 ;;
    Darwin-arm64) node_platform=darwin-arm64 ;;
    *) echo "Unsupported platform for Node: $(uname -s)-$(uname -m)" >&2; exit 1 ;;
  esac
  rm -rf "$TOOLS/node"
  mkdir -p "$TOOLS/node"
  curl -fsSL "https://nodejs.org/dist/v$NODE_VERSION/node-v$NODE_VERSION-$node_platform.tar.gz" \
    | tar -xz -C "$TOOLS/node" --strip-components=1
fi

installed_version() {
  grep -m1 '"version"' "$TOOLS/npm/lib/node_modules/$1/package.json" 2>/dev/null | sed -E 's/.*: *"([^"]+)".*/\1/'
}

if [[ "$(installed_version azurite)" != "$AZURITE_VERSION" ]]; then
  npm install -g --prefix "$TOOLS/npm" "azurite@$AZURITE_VERSION"
fi

if [[ "$(installed_version azure-functions-core-tools)" != "$FUNC_VERSION" ]]; then
  npm install -g --prefix "$TOOLS/npm" "azure-functions-core-tools@$FUNC_VERSION" --allow-scripts=azure-functions-core-tools
fi
chmod +x "$TOOLS/npm/lib/node_modules/azure-functions-core-tools/lib/main.js"

dotnet --version
node --version
azurite --version
func --version
