# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Workflow Rules

**Always get explicit user confirmation before committing or creating a PR.** After making code changes, ask the user to test in their browser/IDE. Wait for their explicit go-ahead (e.g. "looks good, submit it") before running `/submit-pr` or creating any git commits. A clean build is not evidence that a feature works — do not assume changes are ready just because automated checks pass.

**Approval is per-commit and never standing.** Being told to commit once does not authorize the next commit, and neither does completing a task that was asked for. This applies equally to pushing, opening PRs, and rewriting history. Do the work, stop at the staging boundary, report what changed, and ask again.

## Build & Run Commands

```bash
# Build the entire solution
dotnet build Project.slnx

# Run via Aspire (starts dashboard + worker + web UI)
dotnet run --project Project.AppHost/Project.AppHost.csproj

# Run worker directly (standalone, no web UI)
dotnet run --project Project.Worker/Project.Worker.csproj
```

## Architecture

This is a .NET 10.0 Aspire reference application showcasing orchestration of a Blazor Server web app, PostgreSQL database (with ASP.NET Identity), and a background worker service.

### Project Dependency Graph

```
Project.AppHost (Aspire orchestrator — PostgreSQL + projects)
├── Project.Worker (Worker Service — BackgroundService + status API)
│   ├── Project.ServiceDefaults (shared Aspire config)
│   └── Project.Data (EF Core + ASP.NET Identity + PostgreSQL)
└── Project.Web (Blazor Server UI — login + dashboard)
    ├── Project.Data
    └── Project.ServiceDefaults
```

### Worker Service

`WorkerService` runs as a `BackgroundService` under Aspire orchestration. Each loop iteration (every 10 seconds) it writes a `WorkerHeartbeat` record to the database. The Worker project also exposes a minimal API:
- `GET /api/status` — returns `{ status, timestamp, iterations, lastHeartbeat }` queried from the database

### Dependency Injection

All services use **Microsoft.Extensions.DependencyInjection** (MS DI) via the Generic Host.

**Worker — `Program.cs`:**
- `AppDbContext` via `UseNpgsql` (PostgreSQL)
- `WorkerService` (hosted service)

**Web — `Program.cs`:**
- `AppDbContext` via `UseNpgsql` (PostgreSQL)
- ASP.NET Identity (`ApplicationUser`, `IdentityRole`)
- `SeedDataService` (hosted service) — applies migrations and seeds initial user
- `HttpClient` "Worker" — service discovery to call worker status API

### Configuration

**Database:** PostgreSQL managed by Aspire (`Project.Data` project). `AppDbContext` inherits `IdentityDbContext<ApplicationUser>`. Migrations are applied on startup by `SeedDataService` in the Web project.

The Postgres resource uses `WithDataVolume()`, so data survives restarts. To reset to an empty database (e.g. after rewriting a migration), stop the app and delete the volume:

```bash
docker volume ls --filter name=postgres --format '{{.Name}}' | xargs -r docker volume rm
```

Nothing sequences the Worker behind the Web project's migration step — both only `WaitFor(postgres)`, which signals connection readiness, not schema readiness. On a database with no schema yet, the Worker's first heartbeat write fails with `42P01 relation does not exist`, logs two EF errors plus a warning, and succeeds on its next 10-second cycle. Web logs one matching error from `MigrateAsync` probing `__EFMigrationsHistory`. This is expected on a first run against an empty volume and clears on subsequent runs.

**Authentication:** ASP.NET Identity with cookie auth. Invite-only — no registration page. Login is static SSR (required for cookie operations; interactive Blazor Server runs over SignalR which can't set cookies). Logout is a minimal API `POST /account/logout` in `Project.Web/Program.cs`, not a Razor page.

**Seed user:** `SeedDataService` creates one admin account, and only when the user table is empty. `SeedUser:Email` / `SeedUser:Password` are intentionally blank in `appsettings.json` — **never commit real values there.** Supply them out of source control:

```bash
dotnet user-secrets set "SeedUser:Email" "you@example.com" --project Project.Web
```

Set the password the same way (or via `SeedUser__Email` / `SeedUser__Password` environment variables). With nothing configured, Development logs a warning and creates no user — the app runs but nobody can sign in. Outside Development, startup throws instead, so a deployment can't silently come up unreachable.

**Brute-force protection:** none. Login passes `lockoutOnFailure: false`, so failed attempts are not counted and Identity's lockout never engages, and there is no rate limiting on the login endpoint. Password guessing is therefore limited only by network speed. This is a deliberate choice for the template, not an oversight — add lockout and/or rate limiting before exposing an instance to untrusted networks. The login form does return one message for every failure mode, so it never reveals which emails are registered.

**Service Discovery:** The Web project discovers the Worker via Aspire service discovery (`https+http://worker`). The dashboard demonstrates two integration patterns side by side: (1) HTTP call to the worker's `/api/status` endpoint via service discovery, and (2) direct database query of the `WorkerHeartbeats` table. Both poll every 5 seconds.
