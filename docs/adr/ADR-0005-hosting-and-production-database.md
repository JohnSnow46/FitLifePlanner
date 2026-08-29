# ADR-0005: Hosting (Render) and production database (PostgreSQL)

**Date:** 2026-08-29
**Status:** Accepted

**Context.**
ADR-0002 deferred the production database engine "to a future ADR made alongside the
hosting decision" — SQLite was accepted for local dev only. ETAP 7 (`docs/roadmap.md`)
needs both decided together, since the hosting platform and the DB engine constrain each
other (e.g. a stateless container host can't keep a SQLite file across deploys/restarts
without a persistent volume). `Api` and `Web` already ship as Docker images
(`src/FitLifePlanner.Api/Dockerfile`, `src/FitLifePlanner.Web/Dockerfile`) exercised
locally via `docker-compose.yml`, with a JWT key and CORS origins already externalized
to environment variables — the natural next step is deploying those same images rather
than introducing a different packaging format for production.

**Decision.**
Host both services on **Render**, deploying the existing Dockerfiles as two web
services (no change to how the images are built). Use **Render's managed PostgreSQL**
(free tier) as the production database, selected via a new `Database:Provider` config
switch (`Sqlite` default, `Postgres` in production) in `Program.cs`.

Because a single EF Core migrations history can't serve two providers (the generated SQL
is provider-specific), Postgres migrations live in a new project,
`FitLifePlanner.Infrastructure.Postgres`, kept separate from the existing SQLite
migrations in `FitLifePlanner.Infrastructure` — see `docs/database.md` §4 for the
mechanics (`IDesignTimeDbContextFactory`, why it's needed, how migrations are generated
and applied). The existing `Database.Migrate()` call at API startup is unchanged and
works against either provider.

**Alternatives considered.**
- *Azure App Service (F1 free tier).* Rejected for now — stricter limits (60 min CPU/day,
  no "Always On") than Render's free web services, and would need a second decision for
  where the database lives (Azure's free database tiers are more limited than Render's).
  Worth revisiting if Azure familiarity becomes a specific goal for this portfolio.
- *Keep SQLite in production*, with the file on a persistent volume (the same pattern
  `docker-compose.yml` already uses locally). Rejected — Render's free web services don't
  guarantee a persistent disk across deploys, and SQLite doesn't handle concurrent writers
  well under a real (if small) multi-request load; Postgres removes both problems and is
  a one-line provider swap per ADR-0002's design intent.
- *Fly.io / Railway.* Rejected — Fly.io's free tier requires a credit card; Railway's free
  tier is a one-time credit, not permanently free. Render needs neither.

**Consequences.**
- Production config, set as environment variables on Render (never committed): 
  `Database__Provider=Postgres`, `ConnectionStrings__Default=<Render Postgres connection
  string>`, `Jwt__Key`, `Cors__AllowedOrigins__0=<Web's Render URL>` — same
  externalization pattern `docker-compose.yml` already uses for `Jwt__Key`.
- Two migrations projects must be kept in sync by hand: a schema change needs a migration
  added to `FitLifePlanner.Infrastructure` (SQLite) **and**
  `FitLifePlanner.Infrastructure.Postgres` (Postgres) — see `docs/database.md` §4 for both
  commands. `reviewer`/`reviewer-lite` should flag a migration added to only one project.
- Render's free web services spin down after inactivity (cold start on the first request
  after idling) — acceptable for a portfolio demo, worth a one-line callout in the README
  so a visitor doesn't read the delay as the app being broken.
- Step-by-step setup (Render dashboard steps, env vars, connecting the Postgres add-on):
  `docs/deployment.md`.
