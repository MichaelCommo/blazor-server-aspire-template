# .NET Aspire App Template

Skip the boilerplate. This template gives you a fully wired .NET 10 application with authentication, a database, a background worker, and observability — so you can jump straight to building features.

## What You Get

- **Aspire orchestration** — PostgreSQL container, service discovery between services, health checks, OpenTelemetry tracing and metrics
- **Blazor Server web app** — responsive sidebar layout, Bootstrap 5, interactive dashboard
- **Authentication** — ASP.NET Identity with cookie auth, invite-only (no public registration), admin account seeded from your own configured credentials on first startup, with account lockout and login rate limiting
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
- **Credentials for your login** — see [First Run: Create Your Login](#first-run-create-your-login) below. There is no registration page, so this is the only way to get in.

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

## First Run: Create Your Login

**There is no default account and no registration page.** The app ships with no credentials at all — you create the first (and only) admin account by configuring it before startup. Do this before your first `dotnet run`.

### 1. Set your email and password

Run both commands from the repo root. They write to [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), a per-developer store outside the repo, so your credentials never reach git:

```bash
dotnet user-secrets set "SeedUser:Email" "you@example.com" --project Project.Web
```

```bash
dotnet user-secrets set "SeedUser:Password" "<choose-a-password>" --project Project.Web
```

Replace `<choose-a-password>` with a password you pick. It must satisfy the policy in `Project.Web/Program.cs`:

- at least **6 characters**
- at least one **uppercase** letter
- at least one **lowercase** letter
- digits and symbols are allowed, but not required

### 2. Start the app

```bash
dotnet run --project Project.AppHost/Project.AppHost.csproj
```

The account is created during startup. Open the Web URL from the Aspire dashboard and sign in with the email and password you just set.

### What if I get it wrong?

Seeding runs whenever the **user table is empty**, which makes most mistakes recoverable by just setting the secrets and restarting.

| Situation | What happens | How to fix |
|---|---|---|
| You started without setting the secrets | App runs, no account exists, you can't sign in. Development logs a warning; any other environment fails at startup instead. | Set both secrets, then restart. The account is created on the next run. |
| Your password was rejected by the policy | No account is created. The Web logs show `Failed to create seed user:` with the specific rule you missed. | Set a compliant password, then restart. |
| You signed in once and forgot the password | The user table is no longer empty, so seeding is skipped from then on. | Reset the database volume (below), then start again with new secrets. |

To wipe the database and start completely fresh, stop the app and delete the volume:

```bash
docker volume ls --filter name=postgres --format '{{.Name}}' | xargs -r docker volume rm
```

### Deploying

User secrets are a local-development mechanism and are not read in production. Supply the same two values as environment variables instead:

```
SeedUser__Email=you@example.com
SeedUser__Password=...
```

Note the **double underscore** — that's how .NET maps environment variables onto nested configuration keys. Never put real values in `appsettings.json`; it is committed, and the blank entries there exist only to document the shape.

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
4. **Set your own seed credentials** via user secrets — see [First Run](#first-run-create-your-login). Leave `appsettings.json` blank.
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
