# Database

## Index

- Storage choice → §1
- Schema / entities → §2
- Migrations → §3
- Production provider (Postgres) → §4

## 1. Storage choice

EF Core (Code-First, migrations), SQLite as the local-dev provider. Production uses
PostgreSQL — see §4 and ADR-0005 (`docs/adr/ADR-0005-hosting-and-production-database.md`).
Local-dev rationale and alternatives: ADR-0002 (`docs/adr/ADR-0002-data-storage.md`).

## 2. Schema / entities

MVP domain model, one owner (`User`) per row unless noted "shared/global". Full column-
level detail (types, constraints, indexes) is left to the EF Core configurations built
by `builder`, not spelled out here.

**User** — Id, Name, Email, PasswordHash. Owns plans and logs below. `PasswordHash`
(string, required) and the unique index on `Email` come from the JWT auth decision —
ADR-0003 / `docs/api.md` §3; no other credential/role columns at MVP.

**Workout planning**
- `Exercise` (shared/global catalog) — Id, Name, MuscleGroup, Description. Not
  user-owned; reused across all plans.
- `WorkoutPlan` — Id, UserId, Name, CreatedAt. A user's reusable workout template.
- `WorkoutPlanExercise` (join) — WorkoutPlanId, ExerciseId, Order, TargetSets,
  TargetReps, TargetWeight. Defines an exercise's role within a plan.

**Meal/nutrition planning**
- `Food` (shared/global catalog) — Id, Name, Unit, CaloriesPerUnit, ProteinPerUnit,
  CarbsPerUnit, FatPerUnit. Not user-owned.
- `MealPlan` — Id, UserId, Name.
- `MealPlanEntry` (join) — MealPlanId, FoodId, MealType (Breakfast/Lunch/Dinner/Snack),
  Quantity, DayOfWeek.

**Progress tracking** — records of what actually happened, always dated and always
user-owned; this is what trend views are built from.
- `WorkoutLog` — Id, UserId, Date, Notes, WorkoutPlanId (nullable — ad-hoc logging
  without a plan is allowed).
- `WorkoutLogEntry` (child of `WorkoutLog`) — ExerciseId, SetsCompleted, RepsCompleted,
  WeightUsed.
- `MealLog` — Id, UserId, Date, MealType, FoodId, QuantityConsumed.
- `BodyMetricEntry` — Id, UserId, Date, Weight, BodyFatPercent (nullable), Notes.

**Key rules**
- `Exercise` and `Food` are shared reference data (not per-user) — kept simple
  deliberately; no per-user custom catalog at MVP scope.
- Logs (`WorkoutLog`, `MealLog`, `BodyMetricEntry`) are independent of plans/catalog
  lifecycle: deleting a `WorkoutPlan` must not delete historical `WorkoutLog` rows
  (nullable FK, not cascade-delete) — history has to survive plan changes for progress
  tracking to mean anything.
- All log/plan entities carry `UserId`; queries are always scoped to the current user
  (no cross-user data access) even though multi-user auth itself is deferred.

## 3. Migrations

EF Core migrations, generated and applied from the repo root:

```
dotnet ef migrations add <Name> --project src/FitLifePlanner.Infrastructure --startup-project src/FitLifePlanner.Api
dotnet ef database update --project src/FitLifePlanner.Infrastructure --startup-project src/FitLifePlanner.Api
```

The SQLite `.db` file is local and git-ignored — each clone/dev applies migrations to
get a fresh database, no shared dev database. See `CLAUDE.md` Commands for the
day-to-day shortlist.

## 4. Production provider (Postgres)

`Database:Provider` (config, default `Sqlite`) selects the engine in
`Program.cs`; setting it to `Postgres` (via `Database__Provider=Postgres` in the
hosting environment, see `docs/deployment.md`) switches `AddDbContext` to `UseNpgsql`.
Rationale: ADR-0005.

A single EF Core migrations history can't serve two providers — the generated SQL is
provider-specific (e.g. SQLite's `PRAGMA`-driven table rebuilds vs. Postgres `SERIAL`/
`IDENTITY` columns), and EF discovers every `[Migration]`-tagged class in the configured
migrations assembly regardless of which provider generated it. So Postgres migrations
live in their own project/assembly, `FitLifePlanner.Infrastructure.Postgres`, generated
against the same `FitLifePlannerDbContext`/model but kept separate on disk:

```
dotnet ef migrations add <Name> --project src/FitLifePlanner.Infrastructure.Postgres --startup-project src/FitLifePlanner.Infrastructure.Postgres -o Migrations
```

That project's `PostgresDesignTimeDbContextFactory` (an `IDesignTimeDbContextFactory`)
is what `dotnet ef` uses to build the model — it takes precedence over `Program.cs`'s
own host-based discovery, which matters because the API's startup path calls
`Database.Migrate()` before `Run()`; without the factory, `dotnet ef migrations add`
would try to actually apply migrations to a live Postgres connection that doesn't exist
at generation time. The factory's connection string is a placeholder, used only to pick
the provider for SQL generation — never a real target.

At runtime, `Database.Migrate()` in `Program.cs` (unchanged) applies whichever
provider's migrations match the active `DbContext` configuration — no separate deploy
step beyond setting `Database:Provider` and `ConnectionStrings:Default`.
