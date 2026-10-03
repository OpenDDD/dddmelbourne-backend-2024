#!/usr/bin/env bash
# Stop background Functions host and Azurite.
source "$(dirname "$0")/env.sh"
for name in func azurite; do
  if [[ -f "$RUN/$name.pid" ]]; then
    pid="$(cat "$RUN/$name.pid")"
    pkill -P "$pid" 2>/dev/null || true
    kill "$pid" 2>/dev/null || true
    rm -f "$RUN/$name.pid"
  fi
done
pkill -f "func host start --port $FUNC_PORT" 2>/dev/null || true
exit 0
