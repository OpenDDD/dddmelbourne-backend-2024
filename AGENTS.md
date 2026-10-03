# Agent instructions

Azure Functions (.NET 6, in-proc, v4) backend for DDD Melbourne. Projects: `DDD.Core`, `DDD.Functions`, `DDD.Functions.Extensions`, `DDD.Sessionize`, `DDD.Sessionize.Tests`.

## Verify changes

Run `scripts/agent/loop.sh` after every code change. It installs tooling into `.local-tools/` (no sudo), builds, runs unit tests, starts Azurite and the Functions host, smoke-tests the HTTP endpoints in two phases, then stops everything. Exit code 0 means pass.

- Individual steps: `setup.sh`, `test.sh`, `start.sh` (`PHASE=voting|agenda`), `verify.sh`, `stop.sh`.
- Logs are in `.local-run/func.log` and `.local-run/azurite.log`. Read them when a check fails.
- When adding or changing an HTTP function, update `CHECKS` in `scripts/agent/verify.sh`.
- Endpoints are gated by dates, so `start.sh` overrides them via env vars (they take precedence over `local.settings.json`). Do not edit dates in `local.settings.json` for testing.
- Cosmos DB (`UserVotingSessions*`) is not available locally without Docker; those paths are not covered.

## Gotchas

- Do not upgrade `azure-functions-core-tools` past the version pinned in `setup.sh`; newer releases fail on Node 18.
- Never commit `local.settings.json` secrets or `.local-tools/`, `.local-run/`.
- In the VS Code Flatpak sandbox, `git push` over SSH and `gh` may be unavailable; `gh` is at `/run/host/usr/bin/gh`. Ask the user before pushing or opening PRs.
