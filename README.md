# MnemoToad.Knowledge

Backend service for the knowledge-graph domain of MnemoToad — node types, knowledge nodes,
relationship types, and the relations between nodes. ASP.NET Core Web API backed by Postgres.

This is one of several services that make up the MnemoToad platform: a **Learning** service
(flashcards, Leitner boxes, and other study modes) and a consumer-facing **Application** service
(web + mobile) live in their own separate repos and are not part of this one.

## Stack

- **.NET 10**, ASP.NET Core Web API (controllers, no minimal APIs)
- **EF Core** + Npgsql, against **PostgreSQL**
- **DbUp** for schema migrations (plain numbered SQL scripts, not EF Core Migrations)
- **NUnit** + **Moq** for .NET tests
- **Karate** (Java/Maven) for HTTP-level integration tests

## Solution layout

| Project | Purpose |
|---|---|
| `MnemoToad.Knowledge.Api` | ASP.NET Core Web API — controllers, contracts, DI setup |
| `MnemoToad.Knowledge.Data` | Entities, `AppDbContext`, repositories |
| `MnemoToad.Knowledge.DbMigrator` | Standalone DbUp console app that applies SQL migrations |
| `MnemoToad.Knowledge.Tests` | NUnit/Moq controller, repository, and system tests |
| `MnemoToad.Knowledge.Karate` | Java/Maven HTTP integration tests (separate toolchain) |

See [CLAUDE.md](CLAUDE.md) for the full set of project conventions (API patterns, database
conventions, CI/CD pipeline structure, testing approach, etc.).

## Running locally

1. Start Postgres: `docker compose up -d`
2. Add your own `appsettings.Development.json` to `MnemoToad.Knowledge.Api/` and
   `MnemoToad.Knowledge.DbMigrator/` with a connection string pointing at the local database
   (these files are gitignored — each project reads its own, not shared).
3. Apply migrations: `dotnet run --project MnemoToad.Knowledge.DbMigrator`
4. Run the API: `dotnet run --project MnemoToad.Knowledge.Api`

## Testing

- `dotnet test MnemoToad.Knowledge.Tests/MnemoToad.Knowledge.Tests.csproj`
- Karate (from `MnemoToad.Knowledge.Karate/`): `mvn test -Dkarate.env=dev -Dkarate.tags=@Smoke`
