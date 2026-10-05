#!/usr/bin/env bash
# Start Azurite and the Functions host in the background; wait until the host is ready.
# Logs: .local-run/azurite.log, .local-run/func.log
set -euo pipefail
source "$(dirname "$0")/env.sh"
"$(dirname "$0")/stop.sh" >/dev/null 2>&1 || true

mkdir -p "$RUN/azurite"
nohup azurite --silent --skipApiVersionCheck --location "$RUN/azurite" --debug "$RUN/azurite-debug.log" \
  >"$RUN/azurite.log" 2>&1 &
echo $! >"$RUN/azurite.pid"

# Env vars win over local.settings.json: keep time windows open and use Azurite everywhere.
export AgendaScheduleConnectionString="UseDevelopmentStorage=true"
# PHASE=voting (default): submissions open, agenda closed. PHASE=agenda: submissions closed, agenda open.
if [[ "${PHASE:-voting}" == "agenda" ]]; then export SubmissionsAvailableTo="2000-01-02T00:00:00+00:00"; else export SubmissionsAvailableTo="2099-12-31T00:00:00+00:00"; fi
export VotingAvailableTo="2099-12-31T00:00:00+00:00"

cd "$ROOT/DDD.Functions"
nohup func host start --port "$FUNC_PORT" --verbose >"$RUN/func.log" 2>&1 &
echo $! >"$RUN/func.pid"

for _ in $(seq 1 120); do
  if ! kill -0 "$(cat "$RUN/func.pid")" 2>/dev/null; then
    echo "func host exited early:"; tail -n 40 "$RUN/func.log"; exit 1
  fi
  if grep -q "Job host started" "$RUN/func.log" 2>/dev/null; then
    echo "Host ready at $BASE_URL"; exit 0
  fi
  sleep 2
done
echo "Timed out waiting for host:"; tail -n 40 "$RUN/func.log"; exit 1
