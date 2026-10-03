#!/usr/bin/env bash
# One full cycle: build + unit tests -> start host -> smoke tests -> stop. Exit code = verify result.
set -uo pipefail
dir="$(dirname "$0")"
"$dir/setup.sh" || exit 1
"$dir/test.sh" || { echo "LOOP: build/tests failed"; exit 1; }
rc=0
for phase in voting agenda; do
  echo "== PHASE=$phase"
  export PHASE=$phase
  "$dir/start.sh" || { "$dir/stop.sh"; echo "LOOP: host failed to start"; exit 1; }
  "$dir/verify.sh" || rc=1
  "$dir/stop.sh"
done
echo "LOOP: $([[ $rc -eq 0 ]] && echo PASSED || echo FAILED)"
exit $rc
