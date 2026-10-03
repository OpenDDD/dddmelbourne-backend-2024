#!/usr/bin/env bash
# Smoke-test the running host. Exits non-zero if any check fails.
# Expectations are keyed to DDD.Functions/local.settings.json; edit CHECKS when behaviour changes.
source "$(dirname "$0")/env.sh"
fail=0

# name | method | path | body | accepted status codes (space separated)
CHECKS=(
  "GetAgenda (closed while submissions open)|GET|/api/GetAgenda||404"
  "GetAgendaSchedule (no blob yet)|GET|/api/GetAgendaSchedule||404"
  "GetSubmissions|GET|/api/GetSubmissions||200"
  "SubmitVote (empty body is rejected)|POST|/api/SubmitVote|{}|400"
  "SubmitFeedback (empty body is rejected)|POST|/api/SubmitFeedback|{}|400"
  "EloVotingGetPair (no session cookie)|GET|/api/EloVotingGetPair||400"
  "Unknown route returns 404|GET|/api/DoesNotExist||404"
)

if [[ "${PHASE:-voting}" == "agenda" ]]; then
  CHECKS=(
    "GetAgenda|GET|/api/GetAgenda||200"
    "GetSubmissions (closed)|GET|/api/GetSubmissions||404"
    "Unknown route returns 404|GET|/api/DoesNotExist||404"
  )
fi

for c in "${CHECKS[@]}"; do
  IFS='|' read -r name method path body ok <<<"$c"
  args=(-s -o "$RUN/last-body.txt" -w '%{http_code}' -X "$method" --max-time 30 "$BASE_URL$path")
  [[ -n "$body" ]] && args+=(-H 'Content-Type: application/json' -d "$body")
  code="$(curl "${args[@]}" || echo 000)"
  if [[ " $ok " == *" $code "* ]]; then
    echo "PASS $name -> $code"
  else
    echo "FAIL $name -> $code (expected: $ok)"; head -c 300 "$RUN/last-body.txt"; echo; fail=1
  fi
done

# Functions indexed by the host
for fn in GetAgenda GetSubmissions SubmitVote TitoWebhook SessionizeReadModelSync; do
  if grep -q "$fn" "$RUN/func.log"; then echo "PASS indexed $fn"; else echo "FAIL not indexed $fn"; fail=1; fi
done

# Timer syncs hit external APIs/Cosmos and are expected to fail locally.
unexpected="$(grep -E "Exception while executing function" "$RUN/func.log" \
  | grep -vE "Sessionize|Tito|AppInsights|Cosmos|NewSessionNotification" || true)"
if [[ -n "$unexpected" ]]; then
  echo "FAIL host log contains unexpected function exceptions:"
  echo "$unexpected" | head -5; fail=1
fi

exit $fail
