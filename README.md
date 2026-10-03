# DDD Backend

This project contains backend functionality to run the DDD conferences, including:

* Syncing data from [Sessionize](https://sessionize.com/) to Azure Table Storage (tenanted by conference year) for submitted sessions (and submitters) and separate to that, selected sessions (and presenters)
* APIs that return submission and session (agenda) information during allowed times
* APIs to facilitate voting by the community against (optionally anonymous) submitted sessions (notes stored to Azure Table Storage tenanted by conference year) including various mechanisms to detect fraud
* Syncing Tito order IDs and Azure App Insights voting user IDs to assist with voting fraud detection and validation
* API to return analysed voting information
* Ability to trigger an Azure Logic App when a new session is detected from Sessionize (which can then be used to create Microsoft Teams / Slack notifications for visibility to the organising committee and/or to trigger moderation actions)
* Tito webhook to take order notifications, de-duplicate them and place them in queue storage so they can be picked up by a Logic App (or similar) to do things like create Microsoft Teams / Slack notifications for visibility to the organising committee
* Getting feedback information and prize draw names

## Run locally

### Prerequisites

* The .NET 10 SDK. `global.json` pins the SDK version.
* Bash and `curl`. The scripts in `scripts/agent/` install the other tools into `.local-tools/`. They do not need `sudo`.

`scripts/agent/setup.sh` installs these tools:

* The .NET 10 SDK, if it is not already installed in `.local-tools/`
* Node.js
* [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite), which emulates Azure Table, Queue and Blob Storage
* [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)

### Commands

Run the full local loop:

```sh
scripts/agent/loop.sh
```

The loop installs the tools, builds the solution and runs the unit tests. Then it starts Azurite and the Functions host, runs smoke tests against the HTTP endpoints, and stops everything. Exit code 0 means that all checks passed.

You can also run each step:

* `scripts/agent/setup.sh`: Install the tools.
* `scripts/agent/test.sh`: Build the solution and run the unit tests.
* `scripts/agent/start.sh`: Start Azurite and the Functions host on `http://localhost:7071`. Set `PHASE=voting` or `PHASE=agenda` to open the matching date windows.
* `scripts/agent/verify.sh`: Run the smoke tests against the running host.
* `scripts/agent/stop.sh`: Stop the Functions host and Azurite.

The logs are in `.local-run/func.log` and `.local-run/azurite.log`.

`DDD.Functions/local.settings.json` holds the local app settings. The Cosmos DB paths (`UserVotingSessions*` settings, used by `EloVotingGetPair`) need the [Cosmos DB emulator](https://learn.microsoft.com/azure/cosmos-db/emulator), so the local loop does not cover them.

## Structure

* `DDD.Core`: Cross-cutting logic and core domain model
* `DDD.Functions`: Azure Functions project (.NET 10, isolated worker) that contains:
  * `AppInsightsSync`: C# Azure Function that syncs app insights user IDs to Azure Table Storage for users that submitted a vote
  * `TitoSync`: C# Azure Function that syncs Tito order IDs to Azure Table Storage for a configured event
  * `GetAgenda`: C# Azure Function that returns sessions and presenters that have been approved for agenda
  * `GetAgendaSchedule`: C# Azure Function that returns agenda schedule from sessionize
  * `GetSubmissions`: C# Azure Function that returns submissions and submitters for use with either voting or showing submitted sessions
  * `GetVotes`: C# Azure Function that returns analysed vote information; can be piped into Microsoft Power BI or similar for further processing and visualisation
  * `NewSessionNotification`: C# Azure Function that responds to new submissions in Azure Table Storage and then calls a Logic App Web Hook URL (from config) with the session and presenter information (marking that session as notified to avoid duplicate notifications)
  * `SessionizeReadModelSync`: C# Azure Function triggered by a cron schedule defined in config that performs a sync from Sessionize to Azure Table Storage for submissions
  * `SessionizeAgendaSync`: C# Azure Function triggered by a cron schedule defined in config that performs a sync from Sessionize to Azure Table Storage for approved sessions
  * `SubmitVote`: : C# Azure Function that allows a vote for submissions to be submitted, where it is validated and persisted to Azure Table Storage
* `DDD.Functions.Extensions`: App settings, repository set-up and request helpers for `DDD.Functions`
* `DDD.Sessionize`: Syncing logic to sync data from sessionize to Azure Table Storage
* `DDD.Sessionize.Tests`: Unit tests for the Sessionize Syncing code and the encryption helper
* `infra`: Bicep template and deploy script for the Function App. See [infra/README.md](infra/README.md).
* `scripts/agent`: Scripts for the local build, test and smoke-test loop
* `.github/workflows`: GitHub Actions workflows for pull requests and deployment
* `votes-export-to-csv`: Tool that exports the vote tables to CSV

## Backend date parameters and usage

* `StopSyncingSessionsFrom`: this is when we should stop syncing sessions from Sessionize, usually CFP close date
* `StopSyncingAgendaFrom`: this is when we should stop syncing agenda from Sessionize, usually conference date
* `StopSyncingTitoFrom`: this is when we should stop syncing tickets holder information from Tito usually the date before conference date
* `VotingAvailableFrom`: voting start date
* `VotingAvailableTo`: voting end date
* `SubmissionsAvailableFrom`: this is when we can retrieve submitted submissions for internal usage and voting, usually it is the when voting opens
* `SubmissionsAvailableTo`: this is when we cannot retrieve submitted submissions, usually it is when Agenda is published
* `StartSyncingAppInsightsFrom`: this is when we start collecting insights for voting and submissions, usually when CFP opens
* `StopSyncingAppInsightsFrom`: this is when we stop collecting insights for voting and submissions, usually when voting closes
* `FeedbackAvailableFrom`: this is when we start accepting feedback, usually the conference start date at 8:00am
* `FeedbackAvailableTo`: this is when we stop accepting feedback, usually the conference start date at 5:00pm

## Infrastructure Prerequisites

The backend application depends on programmatic access to the [Frontend Website's](https://github.com/dddwa/dddperth-website) Application Insights to pull and store information on voting behavior.

To supply this access, create an API key with `Read telemetry` permissions within the frontend website's Application Insights instance in the Azure Portal, and enter the Application ID and Key presented into the `AppInsightsApplicationId` and `AppInsightsApplicationKey` parameters.

## Voting session settings

* `UserVotingSessionsConnectionString`: connection string of the Cosmos DB NoSQL account that keeps the Elo voting sessions
* `UserVotingSessionsDatabaseId`: database name in that account
* `UserVotingSessionsContainerId`: container name in that database
* `UserVotingSessionHeaderName`: name of the request header that holds the voting session ID. If you do not set it, the API uses `X-DDDPerth-VotingSessionId`.
* `UserVotingSessionTtlSeconds`: time to live of a voting session, in seconds. If you do not set it, the API uses `259200` (3 days).

## Deployment

GitHub Actions builds, tests and deploys the code:

* `.github/workflows/pr.yml` builds the solution and runs the unit tests for each pull request to `master`.
* `.github/workflows/deploy.yml` runs for each push to `master`. It builds, tests and publishes `DDD.Functions`. Then it deploys the package to the Function App `dddmelb-2024-api`.

The deploy workflow signs in to Azure with OpenID Connect (OIDC). It does not use a publish profile or a stored secret. The workflow reads the repository variables `AZURE_CLIENT_ID`, `AZURE_TENANT_ID` and `AZURE_SUBSCRIPTION_ID`. The federated credential trusts only `refs/heads/master`.

The workflow deploys code only. `infra/` defines the Function App and its resources. To change the infrastructure, see [infra/README.md](infra/README.md).

## New Session Notification Logic App

The `NewSessionNotificationLogicAppUrl` value is gotten by creating a logic app and copying the webhook URL from it. The logic app would roughly have:

* `When a HTTP request is received` trigger with json schema of:

    ```json
    {
        "properties": {
            "Presenters": {
                "items": {
                    "properties": {
                        "Bio": {
                            "type": "string"
                        },
                        "ExternalId": {
                            "type": "string"
                        },
                        "Id": {
                            "type": "string"
                        },
                        "Name": {
                            "type": "string"
                        },
                        "ProfilePhotoUrl": {
                            "type": "string"
                        },
                        "Tagline": {
                            "type": "string"
                        },
                        "TwitterHandle": {
                            "type": "string"
                        },
                        "WebsiteUrl": {
                            "type": "string"
                        }
                    },
                    "required": [
                        "Id",
                        "ExternalId",
                        "Name",
                        "Tagline",
                        "Bio",
                        "ProfilePhotoUrl",
                        "WebsiteUrl",
                        "TwitterHandle"
                    ],
                    "type": "object"
                },
                "type": "array"
            },
            "Session": {
                "properties": {
                    "Abstract": {
                        "type": "string"
                    },
                    "CreatedDate": {
                        "type": "string"
                    },
                    "ExternalId": {
                        "type": "string"
                    },
                    "Format": {
                        "type": "number"
                    },
                    "Id": {
                        "type": "string"
                    },
                    "Level": {},
                    "MobilePhoneContact": {},
                    "PresenterIds": {
                        "items": {
                            "type": "string"
                        },
                        "type": "array"
                    },
                    "Tags": {
                        "type": "array"
                    },
                    "Title": {
                        "type": "string"
                    }
                },
                "type": "object"
            }
        },
        "type": "object"
    }
    ```

* `For each` action against `Presenters` with a nested `Compose` action against `Name`
* `Post message` action (for Teams/Slack) with something like `@{join(actionOutputs('Compose'), ', ')} submitted a talk '@{triggerBody()?['Session']['Title']}' as @{triggerBody()?['Session']['Format']} / @{triggerBody()?['Session']['Level']} with tags @{join(triggerBody()?['Session']['Tags'], ', ')}.`
* `Send an email` action (for O365/GMail/Outlook.com depending on what you have) that sends an email if the previous step failed (via `Configure run after`)

## Tito notification logic app

The logic app would roughly have:

* `When there are messages in a queue` trigger to the `attendees` queue of the storage account in the `TitoWebhookConnectionString` app setting
* `Post message` action (for Teams/Slack) with something like `@{json(trigger().outputs.body.MessageText).name} is attending @{json(trigger().outputs.body.MessageText).event} as @{json(trigger().outputs.body.MessageText).ticketClass} (orderid: @{json(trigger().outputs.body.MessageText).orderId}). @{json(trigger().outputs.body.MessageText).qtySold}/@{json(trigger().outputs.body.MessageText).totalQty} @{json(trigger().outputs.body.MessageText).ticketClass} tickets taken.`
* `Delete message` action for the `attendees` queue with the Message ID and Pop Receipt from the trigger

## Prepping for a new year conference

You'd usually would do this before voting opens, that's where the backend needed.

1. Recreate Cosmos Tables in `dddmelb2024`, so they are empty
  * Set maximum 1000 RSU, so they aren't expensive
  * You can leave feedback tables there

![Creating new cosmos table](./docs/new-cosmos-table.png)

2. Cleanup Cosmos NoSQL `dddmelb2024nosql`
  * delete and re-create `votesessions` table, so we can get rid of the data
  * use `/pk` for primary key

![new nosql cosmos table](docs/new-cosmos-nosql-table.png)


3. Create two sessionize keys: one for `voting` and one for `agenda`
  * For `voting` pick:
    * Format - JSON
    * Includes Sessions - All except declined
  * For `agenda` pick:
    * Format - JSON
    * Includes Sessions - Accepted
    * Tick everything from the screenshot below

![Sessionize API options for Agenda](docs/agenda-sessionize-api-options.png)


![Sessionize API keys](docs/sessionize-api-keys.png)

4. Update environment variables in Functions App `dddmelb-2024-api`
  * Go to Functions -> Settings -> Environment Variables
  * `SessionizeApiKey` - use voting API id
  * `SessionizeAgendaApiKey` - use agenda API id
  * `StopSyncingAgendaFrom` - conference date
  * `StopSyncingSessionsFrom` - conference date
  * `SubmissionsAvailableTo` - conference date
  * `VotingAvailableTo` - voting close date 

![app variables](docs/app-variables.png)

5. Wait a bit for functions to run and check `Submissions` and `Submitters` tables. They should contain new data.

6. Run website locally and check that voting works

7. Once done some test voting, check that data is populated in "EloVotes" table.