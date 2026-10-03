# Agent instructions

Azure Functions (.NET 10, isolated worker, v4) backend for DDD Melbourne. Projects: `DDD.Core`, `DDD.Functions`, `DDD.Functions.Extensions`, `DDD.Sessionize`, `DDD.Sessionize.Tests`.

## Verify changes

Run `scripts/agent/loop.sh` after every code change. It installs tooling into `.local-tools/` (no sudo), builds, runs unit tests, starts Azurite and the Functions host, smoke-tests the HTTP endpoints in two phases, then stops everything. Exit code 0 means pass.

- Individual steps: `setup.sh`, `test.sh`, `start.sh` (`PHASE=voting|agenda`), `verify.sh`, `stop.sh`.
- Logs are in `.local-run/func.log` and `.local-run/azurite.log`. Read them when a check fails.
- When adding or changing an HTTP function, update `CHECKS` in `scripts/agent/verify.sh`.
- Endpoints are gated by dates, so `start.sh` overrides them via env vars (they take precedence over `local.settings.json`). Do not edit dates in `local.settings.json` for testing.
- Cosmos DB (`UserVotingSessions*`) is not available locally without Docker; those paths are not covered.

## Gotchas

- `setup.sh` pins the .NET SDK channel, Node, Azurite and `azure-functions-core-tools`. Core Tools needs Node 22 or later, so `setup.sh` installs Node into `.local-tools/node`. Do not use the system Node.
- App settings bind to config classes in `DDD.Functions.Extensions`, which are injected through DI. Keep the app setting names the same; production uses them.
- Never commit `local.settings.json` secrets or `.local-tools/`, `.local-run/`.
- In the VS Code Flatpak sandbox, `git push` over SSH and `gh` may be unavailable; `gh` is at `/run/host/usr/bin/gh`. Ask the user before pushing or opening PRs.
