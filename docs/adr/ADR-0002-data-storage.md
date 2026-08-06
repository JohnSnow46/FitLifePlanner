# ADR-0002: Data storage — EF Core + SQLite

**Date:** 2026-08-05
**Status:** Accepted

**Context.**
The domain (users, plan/catalog entities, dated log entries) is naturally relational.
Hosting is explicitly deferred ("we'll see later" — see ADR-0001's context), so the
storage choice needs to work well for **local development first**, without locking in a
production engine prematurely. This is a public GitHub portfolio repo — anyone cloning
it (including future-you on a different machine/OS) should be able to run it with no
locally installed database server or extra infrastructure.

**Decision.**
Use **EF Core, Code-First with migrations**, targeting **SQLite** as the default local
development provider. The database is a single git-ignored file, created/updated via EF
Core migrations (`dotnet ef database update`) — no separate database server to install
or run. Migrations and the `DbContext` live in `FitLifePlanner.Infrastructure` (see
ADR-0001).

EF Core's provider abstraction is relied on deliberately: switching the underlying
engine (e.g. to PostgreSQL or SQL Server) once hosting is decided is a provider package +
connection string + regenerated-migration change, not a data model rewrite. That
provider swap is deferred to a future ADR made alongside the hosting decision — not
decided here.

**Alternatives considered.**
- *SQL Server LocalDB.* Rejected as the default — Windows-only, requires a locally
  installed instance, adds friction for anyone cloning the repo on another OS.
- *PostgreSQL via Docker.* Rejected as the default for now — an extra moving part
  (Docker dependency) not worth taking on before hosting is decided; a reasonable choice
  to revisit at that point.

**Consequences.**
- Local workflow: `dotnet ef migrations add <Name>` / `dotnet ef database update` (see
  `docs/database.md` §3 and `CLAUDE.md` Commands). Each dev/reviewer gets a fresh DB from
  migrations — no shared dev database to coordinate.
- Entity/migration design should avoid SQLite-incompatible constructs (e.g. provider-
  specific computed columns, SQL Server-only functions) so the future provider swap stays
  cheap. SQLite's looser type affinity and limited `ALTER TABLE` support are acceptable
  at MVP scale.
- Choosing a production provider is explicitly out of scope here and tracked as future
  work once hosting is decided (see `docs/roadmap.md`).
