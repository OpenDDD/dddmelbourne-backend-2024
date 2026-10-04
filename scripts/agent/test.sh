#!/usr/bin/env bash
# Build the solution and run unit tests.
set -euo pipefail
source "$(dirname "$0")/env.sh"
cd "$ROOT"
dotnet build DDD.sln --nologo -v q
dotnet test --project DDD.Sessionize.Tests --no-build
