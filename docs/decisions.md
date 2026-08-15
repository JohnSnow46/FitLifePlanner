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
- [ADR-0004](adr/ADR-0004-web-frontend-structure-and-auth.md) — `Web` frontend: JWT in
  `localStorage` behind a `TokenStore` + custom `AuthenticationStateProvider`, bearer
  `DelegatingHandler` on typed `<FeatureArea>ApiClient`s, own `Contracts` records (no
  shared project), no state-management library, and a CORS policy on `Api`.

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

### 2026-08-15 — Post-review data-integrity hardening (pre-ETAP 7)
- ADRs used: ADR-0002 (§ SQLite as local-dev provider — `HasPrecision` on `decimal`
  columns added now as a no-op on SQLite but required before any prod-provider switch;
  two additive migrations generated, not applied, per the deferred-provider decision),
  ADR-0003 (exception→status mapping extended: new narrow `DbUpdateException` → 409
  branch in `GlobalExceptionHandler` for FK-in-use violations, kept separate from the
  existing `ValidationException`/`NotFoundException` 400/404 pair rather than
  reusing/widening either)
- ADRs read but not used: ADR-0001 (no layer/module boundary change — fixes stayed
  within existing `Infrastructure.Configurations`/`Api.Controllers`), ADR-0004 (frontend
  untouched by design — `DateTime.Today`/local-kind issue on the Blazor side flagged by
  the review but explicitly deferred, not fixed here)
- Context: full-app `reviewer` pass (user-requested, ahead of ETAP 7 planning) found 3
  blockers — cascading delete of catalog items (`Exercise`/`Food`) silently wiped other
  users' plan/log data (`DeleteBehavior.Cascade` on 4 FKs, missing on the first pass for
  `WorkoutPlanExercise`/`MealPlanEntry`, caught only on re-verification), a missing
  ownership check on `WorkoutPlanId` when creating a `WorkoutLog`, and unvalidated
  catalog FKs producing 500s instead of 404s on 4 controller actions. Bundled in the same
  migration: `UserId` indexes on 5 owner entities, `[MaxLength]` on request DTOs matching
  EF `HasMaxLength`, and a two-way UTC `ValueConverter` fix (write-side was previously
  identity, so offset dates round-tripped incorrectly). Took 3 agent rounds
  (builder → reviewer found the plan-level cascade gap → builder → reviewer confirmed
  clean) — two EF migrations generated, neither applied to the dev DB yet.
- Left open (not blockers, logged for later): POST-response echo isn't UTC-normalized
  the way GET responses are (`docs/api.md` §1 wire-format requirement not fully met on
  create); no unit test for the new `SqliteErrorCode`/`"FOREIGN KEY"` substring branch in
  `GlobalExceptionHandler` (only indirectly covered via integration 409 tests); test
  suite uses `EnsureCreated()` not `Database.Migrate()`, so it doesn't actually prove the
  migrations are correct (verified manually this round instead).

### 2026-08-14 — ETAP 6 step 3/4: Progress feature UI
- ADRs used: ADR-0004 (§2 wire-model shape — hand-written `Web/Contracts/Progress`
  records matching `Api/Contracts/Progress` 1:1; `MealLogResponse`/
  `CreateMealLogRequest` reuse the existing `Web.Contracts.Nutrition.MealType` via
  `using` rather than redefining it; §2's `JsonStringEnumConverter` requirement reused
  from the Nutrition step's local `JsonSerializerOptions` pattern; §3 typed-client
  pattern reused from `WorkoutsApiClient`/`NutritionApiClient`; §4 auth usage — all
  four pages carry `[Authorize]`; Consequences — mechanical recipe, plus these three
  resources (`WorkoutLog`/`MealLog`/`BodyMetricEntry`) have no `PUT` at all — append-
  only logs, create+delete only, no rename/edit form anywhere)
- ADRs read but not used: ADR-0001/ADR-0003 (consumes the existing Progress endpoints
  as-is, no `Api`/`Domain` change this step), ADR-0002 (no schema change)

### 2026-08-14 — ETAP 6 step 2/4: Nutrition feature UI
- ADRs used: ADR-0004 (§2 wire-model shape — hand-written `Web/Contracts/Nutrition`
  records matching `Api/Contracts/Nutrition` 1:1, plus a client-side `MealType` enum
  duplicating `Domain.Nutrition.MealType` since `Web` has no project reference to
  `Domain`; §2's `JsonStringEnumConverter` requirement — no shared
  `JsonSerializerOptions` existed yet since Workouts had no enums, so a local
  `JsonSerializerOptions` with `JsonStringEnumConverter` was added inside
  `NutritionApiClient` and passed explicitly to every `*AsJsonAsync` call for
  `MealType`/`DayOfWeek`; §3 typed-client pattern reused from `WorkoutsApiClient`;
  Consequences — same mechanical recipe and delete+re-add rule for
  `MealPlanEntry`)
- ADRs read but not used: ADR-0001/ADR-0003 (consumes the existing Nutrition
  endpoints as-is, no `Api`/`Domain` change this step), ADR-0002 (no schema change)

### 2026-08-14 — ETAP 6 step 1/4: Workouts feature UI
- ADRs used: ADR-0004 (§2 wire-model shape — hand-written `Web/Contracts/Workouts`
  records matching `Api/Contracts/Workouts` 1:1; §3 typed-client pattern —
  `WorkoutsApiClient(HttpClient)` registered via `AddHttpClient<WorkoutsApiClient>` +
  `.AddHttpMessageHandler<BearerTokenHandler>()`, `ReadResponseAsync`/
  `ExtractErrorMessageAsync` helpers duplicated rather than extracted to a shared base,
  per the "only one client existed before this" note; §4 auth usage — pages carry
  `[Authorize]`, links live in `NavMenu`'s `<Authorized>` block; Consequences —
  followed the documented mechanical recipe (Contracts → client methods → pages) and
  the "no `PUT` on plan children, edit = delete + re-add" rule for
  `WorkoutPlanExercise`)
- ADRs read but not used: ADR-0001/ADR-0003 (consumes the existing Workouts endpoints
  as-is, no `Api`/`Domain` change this step), ADR-0002 (no schema change)

### 2026-08-13 — ETAP 5 step 4/4: Users API client, Login/Register pages, closes ETAP 5
- ADRs used: ADR-0004 (§2 wire-model shape — hand-written `Web/Contracts/Users` records
  matching `Api/Contracts/Users` 1:1, only the four DTOs the pages need, no
  `UserUpdateRequest`; §3 typed-client pattern — `UsersApiClient(HttpClient)` registered via
  `AddHttpClient<UsersApiClient>` + `.AddHttpMessageHandler<BearerTokenHandler>()`, errors
  surfaced as `ApiException` built from `ProblemDetails.detail` with a reason-phrase
  fallback; §4 auth usage — pages call `JwtAuthenticationStateProvider.SignIn` on success and
  rely on `[AllowAnonymous]`/`RedirectToLogin`'s `returnUrl` already wired in step 3/4)
- ADRs read but not used: ADR-0001 (layering only, no new module boundary), ADR-0003
  (consumes the existing Users endpoints/DTOs as-is, no `Api` change this step)
- Note: manual verification surfaced that `TokenService` (ETAP 4) issues the id claim as
  `ClaimTypes.NameIdentifier` (long URI key), not the short `sub` claim ADR-0004 §4
  describes — `JwtAuthenticationStateProvider` silently ends up without a
  `ClaimTypes.NameIdentifier` claim client-side (email claim still parses fine, login/logout/
  refresh all work since nothing today reads that claim). Left unfixed as out of this step's
  scope; worth a follow-up before any feature needs the client-side user id from the token.

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

### 2026-08-07 — ETAP 4 step 5/5: Progress controllers (closes ETAP 4)
- ADRs used: ADR-0003 (dated-log endpoint shape — append-only plus delete, no `PUT` —
  applied to all three `Progress` resources; `WorkoutLog`/`MealLog`/`BodyMetricEntry`
  each got their own controller (`WorkoutLogsController`/`MealLogsController`/
  `BodyMetricEntriesController`) rather than one shared `ProgressController`, since the
  three resources don't nest under each other the way `Exercise`→`WorkoutPlan` does under
  `Workouts`; every query scoped via `User.GetUserId()`, ownership violations surfaced as
  404 via `NotFoundException`; nested `POST /workout-logs/{logId}/entries` and
  `DELETE .../entries/{entryId}` mirror the owned-plan nested-child pattern, calling
  `WorkoutLog.AddEntry` with `.Include(l => l.Entries)` loaded first; `Contracts/Progress/`
  DTOs + `ProgressMappings.ToResponse()/ToDetailResponse()`) and ADR-0001 (controllers
  stay thin, `DbContext` used directly, business invariants stay on the domain
  methods/factories). Mirrors steps 3-4/5's shape, confirmed by `reviewer-lite`. This was
  the last ETAP 4 step — API layer (`docs/api.md`) is now complete for the MVP scope.
- ADRs read but not used: ADR-0002 (storage choice already settled, no schema change in
  this step).

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
