#!/usr/bin/env bash
# Deploys infra/main.bicep. Secret app settings are not in the repo: this script reads them from the live app
# (or from SOURCE_APP on the first run) and passes them to the template as a secure parameter.
set -euo pipefail

RESOURCE_GROUP="${RESOURCE_GROUP:-dddmelb-2024}"
APP_NAME="${APP_NAME:-dddmelb-2024-api}"
SOURCE_APP="${SOURCE_APP:-dddmelb-2024}"
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Flex Consumption rejects these settings, or the template manages them.
EXCLUDE='^(FUNCTIONS_WORKER_RUNTIME|FUNCTIONS_EXTENSION_VERSION|WEBSITE_CONTENTAZUREFILECONNECTIONSTRING|WEBSITE_CONTENTSHARE|WEBSITE_RUN_FROM_PACKAGE|WEBSITE_NODE_DEFAULT_VERSION|WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED|WEBSITE_MAX_DYNAMIC_APPLICATION_SCALE_OUT|WEBSITES_ENABLE_APP_SERVICE_STORAGE|WEBSITE_SKIP_CONTENTSHARE_VALIDATION|WEBSITE_ENABLE_SYNC_UPDATE_SITE|SCM_DO_BUILD_DURING_DEPLOYMENT|ENABLE_ORYX_BUILD|AzureWebJobsStorage|AzureWebJobsStorage__.*|APPLICATIONINSIGHTS_CONNECTION_STRING)$'

if az functionapp show -g "$RESOURCE_GROUP" -n "$APP_NAME" -o none 2>/dev/null; then
  from="$APP_NAME"
else
  from="$SOURCE_APP"
fi
echo "Reading app settings from $from"

APP_SETTINGS_JSON="$(az functionapp config appsettings list -g "$RESOURCE_GROUP" -n "$from" -o json \
  | jq -c --arg re "$EXCLUDE" 'map(select(.name | test($re) | not)) | map({(.name): (.value // "")}) | add // {}')"
export APP_SETTINGS_JSON

# --what-if prints resource IDs only, so it does not print secret app settings.
if [[ "${1:-}" == "--what-if" ]]; then
  az deployment group what-if -g "$RESOURCE_GROUP" -p "$DIR/main.bicepparam" --result-format ResourceIdOnly
else
  az deployment group create -g "$RESOURCE_GROUP" -n "functions-$(date +%Y%m%d%H%M%S)" -p "$DIR/main.bicepparam" \
    --query "properties.outputs" -o json
fi
