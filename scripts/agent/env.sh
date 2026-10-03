#!/usr/bin/env bash
# Shared env for the local run/verify loop. Source this; do not execute.
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
TOOLS="$ROOT/.local-tools"
RUN="$ROOT/.local-run"
export DOTNET_ROOT="$TOOLS/dotnet"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
export FUNCTIONS_CORE_TOOLS_TELEMETRY_OPTOUT=1
export PATH="$TOOLS/dotnet:$TOOLS/npm/bin:$PATH"
FUNC_PORT="${FUNC_PORT:-7071}"
BASE_URL="http://localhost:$FUNC_PORT"
mkdir -p "$RUN"
