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

`MnemoToad.Knowledge.DbMigrator/docker-compose.yml` is shared with `MnemoToad.Learning`'s
equivalent (identical file, same Compose project name and container) — one local Postgres server
hosting a separate database per service, mirroring the shared-server topology used in Azure.
Running it from either repo controls the same container; no need to run it from both. The
container doesn't auto-create any database — each service creates its own via a one-time bootstrap
script.

1. Start Postgres: `docker compose -f MnemoToad.Knowledge.DbMigrator/docker-compose.yml up -d`
2. First time only (fresh volume): create the `mnemotoad_knowledge` database and its roles by
   running `MnemoToad.Knowledge.DbMigrator/Bootstrap/CreateDatabaseAndRoles.sql` against the
   container (e.g.
   `psql -h localhost -U postgres -f MnemoToad.Knowledge.DbMigrator/Bootstrap/CreateDatabaseAndRoles.sql`,
   password `localdevpassword`).
3. Your own `appsettings.Development.json` already exists locally in `MnemoToad.Knowledge.Api/` and
   `MnemoToad.Knowledge.DbMigrator/` (gitignored, not shared) pointing at `localhost:5432` /
   `mnemotoad_knowledge` with the `mnemotoad_knowledge_app`/`mnemotoad_knowledge_admin` roles.
4. Apply migrations: `dotnet run --project MnemoToad.Knowledge.DbMigrator`
5. Run the API: `dotnet run --project MnemoToad.Knowledge.Api`

## Testing

- `dotnet test MnemoToad.Knowledge.Tests/MnemoToad.Knowledge.Tests.csproj`
- Karate (from `MnemoToad.Knowledge.Karate/`): `mvn test -Dkarate.env=dev -Dkarate.tags=@Smoke`
