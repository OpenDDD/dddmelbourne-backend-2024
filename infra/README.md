# Infrastructure

`main.bicep` defines the Function App `dddmelb-2024-api` on a Flex Consumption plan. It also defines the storage account, the role assignments, and an optional custom domain.

## Deploy

Sign in with an account that can create role assignments (Owner or User Access Administrator on the resource group). Then run:

```sh
infra/deploy.sh --what-if
infra/deploy.sh
```

The `--what-if` mode shows resource IDs only. It does not show app setting values.

GitHub Actions does not deploy this template. The deploy identity `github-backend` has Website Contributor on the Function App only.

## App settings

The repository does not contain secret app settings. `deploy.sh` reads the settings from the live app and sends them to the template as a secure parameter. On the first run, it reads them from `dddmelb-2024` (`SOURCE_APP`).

The template manages these settings, and they replace the live values:

- `AzureWebJobsStorage__accountName`
- `APPLICATIONINSIGHTS_CONNECTION_STRING`
- `AzureWebJobs.<timer>.Disabled` for each name in `timerFunctionNames`

To change a different setting, change it on the app. The next deploy keeps it.

## Cutover

1. Merge to `master`. The workflow deploys the code to `dddmelb-2024-api`.
2. Copy the function keys of `GetFeedback`, `GetPrizeDraw`, `GetVotes` and `TitoWebhook` from `dddmelb-2024`. Callers such as the Tito webhook use these keys.
3. Smoke-test `https://dddmelb-2024-api.azurewebsites.net`.
4. In Cloudflare, add these records for `api.dddmelbourne.com`. Set the proxy status to DNS only.
   - `CNAME api` to `dddmelb-2024-api.azurewebsites.net`
   - `TXT asuid.api` with the `customDomainVerificationId` output of `deploy.sh`
5. In `main.bicepparam`, set `customDomain = 'api.dddmelbourne.com'` and `enableTimers = true`.
6. On `dddmelb-2024`, disable the timer functions. Then run `infra/deploy.sh`.
7. Change the website to use `https://api.dddmelbourne.com`.
8. When the old app has no traffic, remove the old resources:
   - Remove the Website Contributor role of `github-backend` on `dddmelb-2024`.
   - Delete the GitHub secret `AZUREAPPSERVICE_PUBLISHPROFILE_33B4EA0901904CE49CD9226DB143C43F`.
   - Delete the Function App `dddmelb-2024` and its plan `ASP-dddmelb2024-97b7`.
   - Delete the storage account `dddmelb2024a182`. It holds only the host storage and the content share of the old app. Do this step only if no app setting of `dddmelb-2024-api` refers to it.

The Cosmos DB accounts `dddmelb2024` (Table API) and `dddmelb2024nosql` keep the application data. Do not delete them.
