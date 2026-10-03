#!/usr/bin/env bash
# Stop background Functions host and Azurite, waiting until they have exited.
source "$(dirname "$0")/env.sh"

pids=()
for name in func azurite; do
  if [[ -f "$RUN/$name.pid" ]]; then
    pid="$(cat "$RUN/$name.pid")"
    pids+=("$pid" $(pgrep -P "$pid" || true))
    rm -f "$RUN/$name.pid"
  fi
done
pids+=($(pgrep -f "func host start --port $FUNC_PORT" || true))

alive() { local p; for p in "${pids[@]}"; do kill -0 "$p" 2>/dev/null && return 0; done; return 1; }

((${#pids[@]})) && kill "${pids[@]}" 2>/dev/null
for _ in $(seq 1 20); do alive || exit 0; sleep 0.5; done
kill -9 "${pids[@]}" 2>/dev/null
for _ in $(seq 1 10); do alive || exit 0; sleep 0.5; done
echo "stop.sh: processes still running: ${pids[*]}" >&2
exit 1
