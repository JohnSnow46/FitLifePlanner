# Decisions — index

This file is an **index and usage log**, not content. Full ADR text lives one-per-file in
`docs/adr/ADR-NNNN.md`. Never write ADR content directly here — see
[docs/README.md](README.md) for why (context-window discipline).

## Index

One line per ADR: number, link, one-sentence summary. Newest at the bottom.

- [ADR-0001](adr/ADR-0001-architecture-and-stack.md) — Backend: ASP.NET Core Web API
  (.NET 10); frontend: Blazor WebAssembly SPA; four-project layout (Domain/
  Infrastructure/Api/Web), no separate Application/CQRS layer.
- [ADR-0002](adr/ADR-0002-data-storage.md) — EF Core (Code-First) with SQLite as the
  local-dev provider; production provider deferred to the hosting decision.
- [ADR-0003](adr/ADR-0003-api-conventions-and-auth.md) — REST conventions for `Api`:
  unversioned `/api/<kebab-plural>` routes on per-feature-area controllers, endpoint
  shape per entity kind (catalog / owned plan + nested children / append-only log),
  exception→status mapping (`ValidationException` 400, new `NotFoundException` 404,
  else 500), `ToResponse()` extension mapping, and JWT bearer auth with
  `User.PasswordHash`.

## ADR Notes

Per-task usage log, newest entry at the top. The main/orchestrating thread appends one
entry here after a **normal/deep** mode task finishes (all agents in the chain done) — see
the "Using the docs" section in `CLAUDE.md`, step 6. Skip in fast mode unless an ADR was
actually used.

Template for a new entry:

```markdown
### YYYY-MM-DD — <short task description>
- ADRs used: ADR-NNNN (<one-line impact on the implementation>)
- ADRs read but not used: ADR-NNNN (<why it didn't apply>)
```

<!-- Newest entries go directly below this line. -->

### 2026-08-06 — ETAP 3 step 3/3: Progress business rules (closes ETAP 3)
- ADRs used: ADR-0001 (business rules kept directly on `WorkoutLog`/`MealLog`/
  `BodyMetricEntry` entities, no Application layer; reused steps 1-2/3's
  `Domain.Common.ValidationException`; shape adapted to `Create` static factories since
  these are dated log records, not child-list owners like `WorkoutPlan`/`MealPlan` —
  `WorkoutLog` additionally got an `AddEntry` method for its `WorkoutLogEntry` children,
  mirroring `AddExercise`/`AddEntry` from steps 1-2).
- ADRs read but not used: ADR-0002 (data storage) — no schema/migration change; the
  `WorkoutLogEntryConfiguration` fluent-mapping fix (`WithMany()` → `WithMany(l =>
  l.Entries)`) confirmed via a scratch probe migration with empty `Up`/`Down`.

### 2026-08-06 — ETAP 3 step 2/3: Nutrition business rules
- ADRs used: ADR-0001 (business rules kept directly on `MealPlan`/`MealPlanEntry`
  entities, no Application layer; reused step 1/3's `Domain.Common.ValidationException`
  instead of a new type, mirroring `WorkoutPlan.AddExercise` exactly).
- ADRs read but not used: ADR-0002 (data storage) — no schema/migration change; the
  `MealPlanEntryConfiguration` fluent-mapping fix (`WithMany()` → `WithMany(p =>
  p.Entries)`) confirmed via a scratch probe migration with empty `Up`/`Down`.

### 2026-08-06 — ETAP 3 step 1/3: Workouts business rules
- ADRs used: ADR-0001 (business rules kept directly on `WorkoutPlan`/`WorkoutPlanExercise`
  entities, no Application layer; `Domain` stays free of EF Core/ASP.NET dependencies —
  new `ValidationException` has no outward references).
- ADRs read but not used: ADR-0002 (data storage) — no schema/migration change in this
  task, confirmed via a scratch probe migration with empty `Up`/`Down`.

### 2026-08-05 — ETAP 1 scaffolding step 3/4: EF Core + SQLite wiring, initial migration
- ADRs used: ADR-0002 (Code-First EF Core + SQLite, git-ignored `.db` file, migrations
  in `FitLifePlanner.Infrastructure` — implemented `FitLifePlannerDbContext`, 11
  `IEntityTypeConfiguration<T>` classes, and the `InitialCreate` migration exactly as
  decided; `WorkoutLog.WorkoutPlanId` configured `DeleteBehavior.SetNull` per
  `docs/database.md` §2's history-survives-plan-deletion rule)

### 2026-08-05 — ETAP 1 scaffolding step 2/4: MVP domain entities
- ADRs used: ADR-0001 (Domain layer purity rule — verified all 12 entities are plain
  POCOs with zero EF Core/Infrastructure/Api references, per the layering decision)

### 2026-08-05 — ETAP 1: architecture analysis (initial stack/layering/storage decision)
- ADRs used: ADR-0001, ADR-0002 (created by this task — first real architecture decision
  for the project; ADR-0001-example.md retired in favor of ADR-0001)
