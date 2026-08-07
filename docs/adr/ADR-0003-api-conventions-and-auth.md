# ADR-0003: API conventions, error mapping and authentication

**Date:** 2026-08-07
**Status:** Accepted

**Context.**
ETAP 1-3 fixed the stack (ADR-0001), storage (ADR-0002) and put the business rules
directly on the `Domain` entities (`WorkoutPlan.AddExercise`, `MealPlan.AddEntry`,
`WorkoutLog.Create`/`AddEntry`, `MealLog.Create`, `BodyMetricEntry.Create` — all throwing
`Domain.Common.ValidationException`). ETAP 4 builds the `Api` layer on top of that, and
`docs/api.md` is still an empty stub. `.claude/skills/project-conventions/SKILL.md`
already fixes DTO naming (`<Entity><Verb>Request` / `<Entity>Response` in
`Api/Contracts/<FeatureArea>/`), one controller per feature area, and
"exceptions + a global `ProblemDetails` middleware, not `Result<T>`" — this ADR does not
relitigate those, it fills the gaps they leave open: the concrete route shape, which
verbs each *kind* of entity gets, the exception→status table, how mapping code is
written, and the authentication mechanism (`docs/database.md` §2 explicitly deferred
"scope every query to the current user" to this decision, and `User` has no credential
field yet).

The domain draws a line between three kinds of entities, and the API surface has to keep
that line visible instead of flattening everything into identical CRUD:
shared catalogs (`Exercise`, `Food`, no owner), owned plans (`WorkoutPlan`, `MealPlan`,
children guarded by domain methods), and dated logs (`WorkoutLog`, `MealLog`,
`BodyMetricEntry`, created only through static `Create` factories).

## Decision

### 1. Routing, versioning, serialization

- **Route shape:** `/api/<resource>` where `<resource>` is the **kebab-case plural** of
  the entity (`/api/exercises`, `/api/workout-plans`, `/api/meal-logs`,
  `/api/body-metrics`). Resources, not feature areas, appear in the URL.
- **Controllers stay one-per-feature-area** (per `project-conventions`):
  `UsersController`, `WorkoutsController`, `NutritionController`, `ProgressController`.
  Because one controller serves several resources, the controller carries
  `[ApiController]` + `[Route("api")]` and **every action declares its full relative
  template** (`[HttpPost("workout-plans/{planId:int}/exercises")]`). No
  `[Route("api/[controller]")]` token routing — the controller name is an internal
  grouping detail and must not leak into URLs.
- **Route parameters are typed** (`{id:int}`) so a non-numeric id is a routing miss
  (404), not a model-binding error.
- **No API versioning at MVP.** No `/v1/` segment, no `Asp.Versioning` package. The only
  consumer is `FitLifePlanner.Web`, deployed together with the API, so a version
  negotiation mechanism has no one to negotiate with. If a breaking change ever needs to
  coexist with the old surface, the current unversioned `/api/...` is treated as v1 and a
  parallel `/api/v2/...` is introduced then — reversal cost is a routing change, not a
  redesign.
- **JSON: ASP.NET Core defaults are kept.** `System.Text.Json`, camelCase property names,
  ISO-8601 dates — no custom `JsonNamingPolicy`, no serializer swap. One deliberate
  addition: `JsonStringEnumConverter` is registered globally so `MealType` and
  `DayOfWeek` travel as strings (`"Breakfast"`, `"Monday"`) rather than integers —
  readable payloads and no silent breakage if an enum member is ever reordered.
- **All `DateTime` values on the wire are UTC.** The domain factories compare `date`
  against `DateTime.UtcNow`, so the API contract must match; clients send UTC.
- **Success status codes:** `200 OK` for GET and for POST endpoints that return a
  computed result (login), `201 Created` + `Location` header for resource creation,
  `204 No Content` for PUT and DELETE.

### 2. Endpoint shape per entity kind

**a) Shared catalogs — `Exercise`, `Food`.** Flat, full CRUD, **not** user-scoped:
`GET /api/exercises`, `GET /api/exercises/{id}`, `POST`, `PUT /api/exercises/{id}`,
`DELETE /api/exercises/{id}` (same for `/api/foods`). These entities have no `UserId`
and no domain factory — plain object construction / property assignment in the
controller is correct. They still require authentication (see §5); there is no admin
role at MVP, so any authenticated user may extend the catalog.

**b) Owned plans — `WorkoutPlan`, `MealPlan`.** The plan itself is a top-level
collection, always filtered by the current user:
`GET /api/workout-plans`, `GET /api/workout-plans/{id}`, `POST /api/workout-plans`,
`PUT /api/workout-plans/{id}` (renames — `Name` only; `UserId` and `CreatedAt` are
immutable), `DELETE /api/workout-plans/{id}`. The plan is created with a plain
constructor (no domain factory exists) with `UserId` taken from the token, never from
the request body.

Children are a **nested sub-resource** and adding one **must** go through the domain
method — that is where the invariants live:

- `POST /api/workout-plans/{planId:int}/exercises` → load the plan *including* its
  children, call `plan.AddExercise(exerciseId, order, targetSets, targetReps,
  targetWeight)`, `SaveChangesAsync`, return `201` with a
  `WorkoutPlanExerciseResponse`. Loading the children is mandatory: `AddExercise`
  enforces uniqueness of `ExerciseId` and of `Order` against the in-memory collection,
  so an unloaded collection would silently pass a duplicate.
- `POST /api/meal-plans/{planId:int}/entries` → `plan.AddEntry(foodId, mealType,
  quantity, dayOfWeek)`, same rules.
- `DELETE /api/workout-plans/{planId:int}/exercises/{planExerciseId:int}` and
  `DELETE /api/meal-plans/{planId:int}/entries/{entryId:int}` remove a child. Both child
  entities have their own surrogate `Id`, so the route uses the **child's** id — not the
  catalog `ExerciseId`/`FoodId`, which would be ambiguous. Removal breaks no invariant
  (the domain rules are all "must not already exist" checks on add), so it is a direct
  EF Core delete of the child row after verifying the parent belongs to the current user;
  no domain method is added for it.
- **No `PUT` on children.** The domain exposes only `AddExercise`/`AddEntry`; changing an
  exercise's target sets/reps is delete + re-add. Adding an update path would mean
  re-implementing the uniqueness checks in a second place, which is exactly what ETAP 3
  avoided.

**c) Dated logs — `WorkoutLog`, `MealLog`, `BodyMetricEntry`.** **Append-only plus
delete: create, read, delete — no `PUT`/`PATCH`.** Creation always goes through the
static factory (`WorkoutLog.Create`, `MealLog.Create`, `BodyMetricEntry.Create`), which
rejects future dates and non-positive quantities. A log is a record of what happened; a
mistyped record is deleted and re-entered rather than edited, which keeps the "validated
exactly once, in the factory" property. Delete is allowed (a wrong entry must be
removable) and breaks no invariant.

- `GET /api/workout-logs?from=&to=`, `GET /api/workout-logs/{id}`,
  `POST /api/workout-logs`, `DELETE /api/workout-logs/{id}`.
- `POST /api/workout-logs/{logId:int}/entries` → `log.AddEntry(exerciseId,
  setsCompleted, repsCompleted, weightUsed)`;
  `DELETE /api/workout-logs/{logId:int}/entries/{entryId:int}`.
- `MealLog` and `BodyMetricEntry` have no children: `GET`(list with `from`/`to`),
  `GET /{id}`, `POST`, `DELETE /{id}`.
- Every list endpoint takes optional `from`/`to` (inclusive, ISO-8601 UTC dates) query
  parameters; omitted means unbounded. This is what the later trend views need, and it
  is cheap to add now rather than reshaping the routes later.
- `UserId` always comes from the token, never from the request body — `<Entity>Request`
  DTOs for logs and plans **do not contain a `UserId` field at all**.

### 3. Exception → HTTP status mapping

A single `GlobalExceptionHandler` (implementing `IExceptionHandler`, registered with
`AddExceptionHandler` + `AddProblemDetails` + `UseExceptionHandler`) in
`src/FitLifePlanner.Api/Middleware/` owns this table. Controllers never catch to build an
error response.

| Thrown | Status | `ProblemDetails` body |
|---|---|---|
| `Domain.Common.ValidationException` | **400 Bad Request** | `title: "Validation failed"`, `detail` = exception message (domain messages are user-safe by design) |
| `Domain.Common.NotFoundException` (new) | **404 Not Found** | `title: "Resource not found"`, `detail` = exception message |
| anything else | **500 Internal Server Error** | `title: "An unexpected error occurred."`, no `detail` outside Development; the exception is logged server-side with the trace id |

- **`NotFoundException` is new and lives next to `ValidationException` in
  `FitLifePlanner.Domain/Common/`**, mirroring its minimal shape (message only, no extra
  properties):
  `public NotFoundException(string entityName, object key) : base($"{entityName} with id
  '{key}' was not found.")`. It sits in `Domain` even though `Api` handlers are its main
  thrower, so both exception types stay in one place and `Domain` can throw it too
  without a new dependency.
- **Ownership violations return 404, not 403.** If a plan or log exists but its `UserId`
  is not the current user, the controller throws `NotFoundException` — the API must not
  reveal that another user's row exists. Concretely this falls out of the query itself:
  every lookup is `.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId)`, and a
  `null` result throws `NotFoundException`.
- **Request DTO validation stays with the framework.** DataAnnotations on
  `<Entity><Verb>Request` + `[ApiController]`'s automatic model-state response produce
  the standard `400` `ValidationProblemDetails`; that behaviour is not suppressed and not
  re-routed through the handler. DataAnnotations cover shape (required, ranges, string
  lengths); business invariants stay in `Domain` and surface as `ValidationException`.
  Both end up as 400, so the client sees one consistent status for "bad input".
- **401/403 are produced by the authentication middleware**, not by the exception
  handler: a missing/expired token yields `401`, and there are no roles at MVP so `403`
  is not used.

### 4. Entity ↔ DTO mapping

- **No mapping library** (no AutoMapper/Mapster) — explicit code only, consistent with
  ADR-0001's "no abstraction without payoff".
- **Entity → response: static extension methods** named `ToResponse()`, grouped one file
  per feature area at
  `src/FitLifePlanner.Api/Contracts/<FeatureArea>/<FeatureArea>Mappings.cs`
  (e.g. `WorkoutsMappings` with `public static WorkoutPlanResponse ToResponse(this
  WorkoutPlan plan)`, `ToResponse(this Exercise exercise)`, …). Collections use
  `.Select(x => x.ToResponse())`. Keeping them out of the controller means the same
  entity mapped from two endpoints can never drift.
- **Request → entity: inline in the controller, no `ToDomain()` mapper.** Domain objects
  are created through their factories/methods with named arguments
  (`WorkoutLog.Create(userId, request.Date, request.Notes, request.WorkoutPlanId)`), and
  `userId` comes from the token, not the DTO — a generic reverse mapper would have to
  either see the token or be bypassed, so it earns nothing.
- Responses never expose `Domain` entities or navigation graphs; a plan's response
  includes its children as nested `…Response` records, a list endpoint returns the parent
  without children.
- DTOs are `record` types with `init` properties, one file per DTO, named per
  `project-conventions` (`<Entity><Verb>Request` / `<Entity>Response`).

### 5. Authentication

**JWT bearer tokens, hand-rolled on top of `Microsoft.AspNetCore.Authentication.JwtBearer`
— no ASP.NET Core Identity.**

- **`User` gains one field: `PasswordHash` (`string`, required, non-nullable).** Nothing
  else — no security stamp, no lockout counters, no roles, no refresh-token table. Email
  stays the login identifier and gets a unique index. This is an additive migration.
- **Hashing:** `Microsoft.AspNetCore.Identity.PasswordHasher<User>` used standalone (the
  class, not the Identity stack) — registered in DI as `PasswordHasher<User>`. It gives a
  vetted PBKDF2 implementation with salt and versioned format for zero design work; the
  alternative was hand-rolling PBKDF2, which is exactly the kind of thing not to
  hand-roll.
- **Endpoints:** `POST /api/auth/register` (name, email, password → creates the `User`,
  returns a token), `POST /api/auth/login` (email, password → token). Both live on
  `UsersController` (auth is part of the `Users` feature area — no separate area is
  invented) and are the only `[AllowAnonymous]` endpoints.
- **Token contents:** HS256, claims `sub` = `User.Id` (as string) and `email`; 7-day
  expiry; **no refresh tokens** — expiry means logging in again. Signing key, issuer and
  audience come from configuration (`Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`); the key is
  stored in user-secrets locally and is **never committed** (this is a public repo).
- **Everything is protected by default:** `Program.cs` sets a fallback authorization
  policy requiring an authenticated user, so a new controller is secure even if someone
  forgets `[Authorize]`; anonymous access is opt-in per action.
- **Resolving "the current user":** a single extension
  `src/FitLifePlanner.Api/Common/ClaimsPrincipalExtensions.cs` →
  `public static int GetUserId(this ClaimsPrincipal principal)`, reading
  `ClaimTypes.NameIdentifier` (the mapped `sub` claim) and throwing
  `InvalidOperationException` when absent — absence means a misconfigured pipeline, not a
  client error, so 500 is the right outcome. **Every** controller action on an owned
  entity starts with `var userId = User.GetUserId();` and every query for a `UserId`-
  carrying entity filters on it. There is no ambient "current user service" and no
  `IHttpContextAccessor` injection; the principal is already on the controller.
- **Failed login returns `401` directly from the action** (with a `ProblemDetails` body)
  rather than throwing — a wrong password is a normal outcome of the login endpoint, not
  an exceptional one, and it must not be distinguishable from "unknown email".
  Registering an already-used email throws `ValidationException` → 400.
- `FitLifePlanner.Web` attaches `Authorization: Bearer <token>` in its typed API clients;
  how the SPA stores the token is a `Web`-side concern decided when that layer is built.

## Alternatives considered

- **Versioned routes (`/api/v1/...`) from day one.** Rejected — one consumer shipped
  together with the API; the segment would be pure ceremony, and adding it later costs a
  route template change.
- **One controller per entity (12 controllers).** Rejected — contradicts
  `project-conventions`' one-controller-per-feature-area rule and would scatter four tiny
  progress controllers; explicit per-action route templates give resource-shaped URLs
  without it.
- **Flat routes for plan children** (`POST /api/workout-plan-exercises` with `planId` in
  the body). Rejected — it hides that the child only exists inside its parent, and it
  makes the "load the parent with its children, then call the domain method" step look
  optional when it is required for the uniqueness invariants to work.
- **Mutable logs (`PUT /api/workout-logs/{id}`).** Rejected — the domain validates only
  in `Create`, so an update path would duplicate the date/quantity rules in the
  controller or force new domain methods; delete + re-create covers the same user need at
  MVP.
- **ASP.NET Core Identity (full stack).** Rejected — 7+ tables, a large migration, and a
  configuration surface far beyond a single-user portfolio app's needs. Only its
  `PasswordHasher<T>` is borrowed.
- **Cookie authentication.** Rejected — the SPA is standalone Blazor WASM served
  potentially from a different origin than the API (ADR-0001 deliberately keeps hosting
  open), which drags in CORS-with-credentials, SameSite and CSRF handling. A bearer token
  is origin-agnostic.
- **No auth at all / a `X-User-Id` dev header.** Rejected — `docs/database.md` §2 already
  commits every query to being user-scoped, and the whole point of deferring auth to this
  step was to pick a real mechanism before controllers hard-code assumptions. A fake
  current user would have to be unpicked from every controller later.
- **`Result<T>` instead of exceptions.** Not reopened — already decided in
  `project-conventions`; this ADR only fills in the status mapping.

## Consequences

- Later ETAP 4 steps are mechanical: the endpoint list in `docs/api.md` §2 is fixed, the
  status table is fixed, and each controller follows the same skeleton (resolve `userId`
  → query/`Include` → call domain method → `SaveChangesAsync` → `ToResponse()`).
- One additive migration is required before the auth step:
  `User.PasswordHash` + a unique index on `User.Email`. Existing rows in a local dev
  database would need re-seeding, which at this stage is a `dotnet ef database update` on
  a throwaway file (ADR-0002).
- New `Api` dependencies: `Microsoft.AspNetCore.Authentication.JwtBearer` and
  `Microsoft.AspNetCore.Identity` (for `PasswordHasher<T>` only). `Domain` and
  `Infrastructure` gain no new packages.
- `Domain` gains one type (`Common/NotFoundException`); no entity changes beyond
  `User.PasswordHash`.
- No editing of plan children or logs means the frontend's "edit" affordances are
  delete + re-add. If that turns out to be painful in the `Web` layer, adding an update
  path is a follow-up ADR plus a domain method — additive, not a redesign.
- The catalog (`Exercise`, `Food`) being writable by any authenticated user is a known
  simplification; introducing roles later means adding a claim and `[Authorize(Roles=…)]`
  on those five endpoints, with no route changes.
- No refresh tokens means a 7-day session ceiling; acceptable for a portfolio app, and
  revisitable without touching endpoints.
