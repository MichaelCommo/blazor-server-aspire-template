# .NET Aspire App Template

Skip the boilerplate. This template gives you a fully wired .NET 10 application with authentication, a database, a background worker, and observability — so you can jump straight to building features.

## What You Get

- **Aspire orchestration** — PostgreSQL container, service discovery between services, health checks, OpenTelemetry tracing and metrics
- **Blazor Server web app** — responsive sidebar layout, Bootstrap 5, interactive dashboard
- **Authentication** — ASP.NET Identity with cookie auth, invite-only (no public registration), seed user created on first startup
- **EF Core + PostgreSQL** — Identity tables, custom entities, automatic migrations on startup
- **Background Worker** — a `BackgroundService` that writes heartbeat records every 10 seconds, plus a status API endpoint
- **Two integration patterns** — the dashboard shows service discovery (HTTP call to Worker API) and shared database access (reading the same table Worker writes to) side by side

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Aspire uses it to run PostgreSQL)
- **Trust the .NET HTTPS dev certificate** (one-time setup):
  ```bash
  dotnet dev-certs https --trust
  ```
- **Set the seed admin credentials** — there is no registration page, so this is the only way in. They are deliberately blank in `appsettings.json`; set them via user secrets so they stay out of source control:
  ```bash
  dotnet user-secrets set "SeedUser:Email" "you@example.com" --project Project.Web
  ```
  Set `SeedUser:Password` the same way. The account is created on first startup, only while the user table is empty.

## Quick Start

Make sure Docker is running, then:

```bash
# Build
dotnet build Project.slnx

# Run the full stack via Aspire
dotnet run --project Project.AppHost/Project.AppHost.csproj
```

This starts everything:

| Service | What it does |
|---------|-------------|
| **Aspire Dashboard** | Resource monitoring — URL printed in console output |
| **PostgreSQL** | Database container, auto-provisioned |
| **Web** | Blazor Server app — open the URL shown in the dashboard |
| **Worker** | Writes a heartbeat every 10s, exposes `GET /api/status` |

### Default Login

A seed user is created on first startup. Credentials are configured in `Project.Web/appsettings.json`:

| Field | Default value |
|-------|--------------|
| Email | `admin@local.host` |
| Password | `Pa55w0rd` |

Change these before deploying anywhere real.

## Project Structure

```
Project.slnx
├── Project.AppHost/          Aspire orchestrator (PostgreSQL + services)
├── Project.Web/              Blazor Server (login, dashboard, layout)
├── Project.Worker/           BackgroundService + status API
├── Project.Data/             EF Core context, entities, migrations
└── Project.ServiceDefaults/  Shared Aspire config (telemetry, discovery, health)
```

## Make It Yours

1. **Clone** this repo and rename the solution/projects to match your app
2. **Update namespaces** — find and replace `Project.` with `YourApp.`
3. **Update `Directory.Build.props`** — set your company name and copyright
4. **Change the seed credentials** in `Project.Web/appsettings.json`
5. **Add your entities** to `Project.Data/Entities/` and register them in `AppDbContext`
6. **Add pages** under `Project.Web/Components/Pages/` — the layout, nav, and auth are ready to go
7. **Add worker jobs** — use `WorkerService.cs` as a pattern for your own background tasks

### Adding a Migration

Any time you change `AppDbContext` — adding a new `DbSet`, modifying an entity's properties, or adding configuration in `OnModelCreating` — you need a new migration so EF Core can update the database schema to match.

```bash
dotnet ef migrations add YourMigrationName \
  --project Project.Data \
  --startup-project Project.Web
```

Migrations run automatically on startup via `SeedDataService`, so you just restart the app after adding one.

## Claude Code

The `.claude/` directory contains pre-configured agents and conventions so that [Claude Code](https://claude.ai/code) understands the codebase from the first prompt:

- **`CLAUDE.md`** — build commands, architecture overview, workflow rules
- **`.claude/agents/`** — specialized agents for UI engineering, architecture decisions, and code review
- **`.claude/commands/`** — PR submission workflow

If you don't use Claude Code, this directory has no effect on anything. Leave it or delete it.

## License

[MIT](LICENSE)
