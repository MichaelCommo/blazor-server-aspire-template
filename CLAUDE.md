# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Workflow Rules

**Always get explicit user confirmation before committing or creating a PR.** After making code changes, ask the user to test in their browser/IDE. Wait for their explicit go-ahead (e.g. "looks good, submit it") before running `/submit-pr` or creating any git commits. Playwright testing by Claude does not substitute for user testing. Do not assume changes are ready just because they look correct in automated checks.

## Build & Run Commands

```bash
# Build the entire solution
dotnet build Project.sln

# Run via Aspire (starts dashboard + worker + web UI)
dotnet run --project Project.AppHost/Project.AppHost.csproj

# Run worker directly (standalone, no web UI)
dotnet run --project Project.Worker/Project.Worker.csproj
```

## Architecture

This is a .NET 9.0 Aspire reference application showcasing orchestration of a Blazor Server web app, PostgreSQL database (with ASP.NET Identity), and a background worker service.

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

**Authentication:** ASP.NET Identity with cookie auth. Invite-only — no registration page. A seed user is created on first startup from `appsettings.Development.json` (`SeedUser:Email` / `SeedUser:Password`). Login is static SSR (required for cookie operations; interactive Blazor Server runs over SignalR which can't set cookies).

**Service Discovery:** The Web project discovers the Worker via Aspire service discovery (`https+http://worker`). The dashboard demonstrates two integration patterns side by side: (1) HTTP call to the worker's `/api/status` endpoint via service discovery, and (2) direct database query of the `WorkerHeartbeats` table. Both poll every 5 seconds.
