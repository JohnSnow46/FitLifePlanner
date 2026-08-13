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

### 2026-08-13 — ETAP 5 step 2/4: token storage, auth state provider, bearer handler
- ADRs used: ADR-0004 (§3/§4 fixed the exact shape implemented: `TokenStore` wrapping
  `IJSRuntime` localStorage under key `fitlife.token` with field caching, no
  `Blazored.LocalStorage`; `JwtAuthenticationStateProvider` decoding the JWT payload
  client-side with no signature check, dropping the token on missing/malformed/expired
  `exp`; `BearerTokenHandler` attaching the bearer header and calling `SignOut()` on 401
  without redirecting; `ApiException(HttpStatusCode, string)` shape; DI registrations
  mapping `AuthenticationStateProvider` to the concrete scoped instance)
- ADRs read but not used: ADR-0001 (layering only, no new module boundary crossed),
  ADR-0003 (backend auth already in place; this step only consumes the JWT shape it
  produces, no `Api` changes)

<!-- Newest entries go directly below this line. -->

### 2026-08-07 — ETAP 4 step 4/5: Nutrition controllers
- ADRs used: ADR-0003 (single `NutritionController` per the one-controller-per-feature-area
  rule, serving both the `Food` catalog — flat CRUD, not user-scoped — and owned
  `MealPlan`/`MealPlanEntry` resources; every owned query scoped via `User.GetUserId()`,
  ownership violations surfaced as 404 not 403; nested
  `POST /meal-plans/{planId}/entries` loads the plan with `.Include(p => p.Entries)`
  before calling `MealPlan.AddEntry`, letting `ValidationException` propagate to
  `GlobalExceptionHandler` uncaught; `Contracts/Nutrition/` DTOs +
  `NutritionMappings.ToResponse()/ToDetailResponse()`, no `Domain` entities returned
  directly) and ADR-0001 (controller stays thin, `DbContext` used directly, business
  invariant stays on the domain method). Mirrors step 3/5's `WorkoutsController` shape
  exactly, confirmed by `reviewer-lite`.
- ADRs read but not used: ADR-0002 (storage choice already settled, no schema change in
  this step).

### 2026-08-07 — ETAP 4 step 3/5: Workouts controllers
- ADRs used: ADR-0003 (single `WorkoutsController` per the one-controller-per-feature-area
  rule, serving both the `Exercise` catalog — flat CRUD, not user-scoped — and owned
  `WorkoutPlan`/`WorkoutPlanExercise` resources; every owned query scoped via
  `User.GetUserId()`, ownership violations surfaced as 404 not 403; nested
  `POST /workout-plans/{planId}/exercises` loads the plan with `.Include(p =>
  p.Exercises)` before calling `WorkoutPlan.AddExercise`, letting `ValidationException`
  propagate to `GlobalExceptionHandler` uncaught; `Contracts/Workouts/` DTOs +
  `WorkoutsMappings.ToResponse()/ToDetailResponse()`, no `Domain` entities returned
  directly) and ADR-0001 (controller stays thin, `DbContext` used directly, business
  invariant stays on the domain method).
- ADRs read but not used: ADR-0002 (storage choice already settled, no schema change in
  this step).

### 2026-08-07 — ETAP 4 step 2/5: auth infrastructure + Users controller
- ADRs used: ADR-0003 (implemented exactly as decided — `NotFoundException` mirroring
  `ValidationException`'s shape, `User.PasswordHash` + unique `Email` index via the
  additive `AddUserAuthFields` migration, `GlobalExceptionHandler` with the
  Validation→400/NotFound→404/else→500 mapping, hand-rolled JWT bearer auth with
  `PasswordHasher<User>`, fallback-authenticated-by-default policy, `ClaimsPrincipal.
  GetUserId()`, and `UsersController` with register/login/me exactly per §2.1/§5) and
  ADR-0001 (controller stays thin, `DbContext` used directly, no repository
  abstraction).
- ADRs read but not used: ADR-0002 (storage choice already settled, nothing new needed
  beyond the additive migration ADR-0003 already covered).

### 2026-08-07 — ETAP 4 step 1/5: API conventions, error mapping and auth (new ADR-0003)
- ADRs used: ADR-0001 (kept the no-Application-layer shape — `Api` controllers call
  domain factories/methods directly; endpoint shape per entity kind mirrors the
  catalog/owned-plan/dated-log split ETAP 3 already established) and ADR-0002 (auth
  migration is additive only — `User.PasswordHash` + unique `Email` index, SQLite local
  dev unaffected).
- ADRs read but not used: none beyond ADR-0001/0002 — this step produced ADR-0003
  itself rather than consuming a prior one.

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
