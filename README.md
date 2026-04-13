# BBScoreboard

![BBScoreboard banner](docs/assets/bbscoreboardbanner.png)

BBScoreboard is a cross-platform ASP.NET Core app for live basketball scoring, gameplay sync, and stats.

## Tech Stack

- .NET 10 (`net10.0`)
- ASP.NET Core Razor Pages + API controllers
- EF Core 10 with dual database support:
  - PostgreSQL (default)
  - SQL Server (supported)
- ASP.NET Core Identity (role-based auth)
- xUnit test suite + CI matrix

## Repository Structure

- `src/BBScoreboard.Web` - web UI + API
- `src/BBScoreboard.Application` - use cases and gameplay services
- `src/BBScoreboard.Domain` - entities, enums, constants
- `src/BBScoreboard.Infrastructure` - EF Core, Identity, migrations
- `src/BBScoreboard.Tests` - unit/API/RBAC/provider tests
- `docs/` - migration and validation checklist

## Prerequisites

- .NET 10 SDK
- Docker Desktop (optional, for compose-based runtime checks)

## Local Setup

1. Copy `.env.example` to `.env` and set real secrets.
2. Restore tools and packages:
   - `dotnet tool restore`
   - `dotnet restore src/BBScoreboard.slnx`
3. Build and test:
   - `dotnet build src/BBScoreboard.slnx -c Release`
   - `dotnet test src/BBScoreboard.slnx -c Release`
4. Run locally (PostgreSQL default):
   - Set env vars:
     - `Database__Provider=postgres`
     - `ConnectionStrings__Postgres=<your-connection-string>`
     - `BootstrapAdmin__Email=<admin-email>`
     - `BootstrapAdmin__Password=<admin-password>`
   - Start app:
     - `dotnet run --project src/BBScoreboard.Web`

## Database Migrations

Migrations are maintained separately per provider:

- `src/BBScoreboard.Infrastructure/Migrations/Postgres`
- `src/BBScoreboard.Infrastructure/Migrations/SqlServer`

Apply migrations manually:

- PostgreSQL:
  - `Database__Provider=postgres ConnectionStrings__Postgres="<conn>" dotnet tool run dotnet-ef database update --project src/BBScoreboard.Infrastructure --startup-project src/BBScoreboard.Web --context PostgresBbScoreboardDbContext`
- SQL Server:
  - `Database__Provider=sqlserver ConnectionStrings__SqlServer="<conn>" dotnet tool run dotnet-ef database update --project src/BBScoreboard.Infrastructure --startup-project src/BBScoreboard.Web --context SqlServerBbScoreboardDbContext`

## Docker

- Default (PostgreSQL):
  - `docker compose up -d --build web`
- SQL Server profile:
  - `docker compose --profile sqlserver up -d --build web-sqlserver`

Make sure these are set in `.env`: `POSTGRES_PASSWORD`, `BOOTSTRAP_ADMIN_EMAIL`, `BOOTSTRAP_ADMIN_PASSWORD`, and for SQL Server profile `MSSQL_SA_PASSWORD`.

## CI

GitHub Actions workflow runs:

- cross-platform build/test (Linux/macOS/Windows)
- runtime smoke checks
- PostgreSQL and SQL Server integration tests
- Docker compose profile smoke checks

See: `.github/workflows/ci.yml`

## Security Notes

- No plaintext runtime passwords are committed in active app config.
- Bootstrap admin is created only when credentials are provided.
- Change bootstrap credentials immediately after first login.

## License

MIT
