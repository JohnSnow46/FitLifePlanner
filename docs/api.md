# API

REST over HTTP/JSON, served by `FitLifePlanner.Api` and consumed by
`FitLifePlanner.Web`. Rationale and alternatives for everything below: ADR-0003
(`docs/adr/ADR-0003-api-conventions-and-auth.md`).

## Index

- Conventions → §1
- Endpoints → §2
- Auth → §3

## 1. Conventions

**Routing.** `/api/<resource>`, where `<resource>` is the kebab-case plural of the
entity (`/api/workout-plans`, `/api/body-metrics`). Controllers are one per feature area
(`Users`, `Workouts`, `Nutrition`, `Progress`) and carry `[ApiController]` +
`[Route("api")]`; every action declares its own full relative template
(`[HttpPost("workout-plans/{planId:int}/exercises")]`). The controller name never appears
in a URL. Route parameters are typed (`{id:int}`).

**Versioning.** None at MVP — no `/v1/` segment, no versioning package. The current
surface is implicitly v1; a future breaking change would introduce `/api/v2/...`
alongside it.

**Serialization.** ASP.NET Core defaults: `System.Text.Json`, camelCase properties,
ISO-8601 dates. One addition — a global `JsonStringEnumConverter`, so `MealType` and
`DayOfWeek` are strings (`"Breakfast"`, `"Monday"`). All `DateTime` values on the wire
are UTC (the domain factories compare against `DateTime.UtcNow`).

**Status codes.** `200` for GET and for POSTs returning a computed result (login);
`201` + `Location` for resource creation; `204` for PUT/DELETE.

**Entity kinds drive endpoint shape.**
- *Shared catalogs* (`Exercise`, `Food`) — flat full CRUD, not user-scoped.
- *Owned plans* (`WorkoutPlan`, `MealPlan`) — user-scoped CRUD; children are a nested
  sub-resource and adding one goes through the domain method (`AddExercise`/`AddEntry`)
  with the parent's children loaded, since the uniqueness invariants are checked against
  the in-memory collection. Children have no `PUT` — change = delete + re-add.
- *Dated logs* (`WorkoutLog`, `MealLog`, `BodyMetricEntry`) — append-only plus delete:
  create (via the static `Create` factory), read, delete. No `PUT`/`PATCH`.

**Ownership.** `UserId` always comes from the token, never from a request body; request
DTOs carry no `UserId`. Every query for a `UserId`-carrying entity filters on it
(`x.Id == id && x.UserId == userId`), and a miss is a **404**, never a 403 — the API does
not reveal that another user's row exists.

**Errors.** One `GlobalExceptionHandler` (`IExceptionHandler` + `AddProblemDetails` +
`UseExceptionHandler`) in `src/FitLifePlanner.Api/Middleware/` maps exceptions to
`ProblemDetails`; controllers never catch to build error responses.

| Thrown | Status |
|---|---|
| `Domain.Common.ValidationException` | 400 Bad Request (`detail` = message) |
| `Domain.Common.NotFoundException` | 404 Not Found (`detail` = message) |
| anything else | 500 Internal Server Error (generic body, logged server-side) |

DataAnnotations on request DTOs + `[ApiController]` produce the standard 400
`ValidationProblemDetails` for shape errors; business invariants stay in `Domain` and
surface as `ValidationException` — both are 400. `401` comes from the auth middleware;
`403` is unused (no roles at MVP).

**DTOs and mapping.** `record` types named `<Entity><Verb>Request` / `<Entity>Response`
in `src/FitLifePlanner.Api/Contracts/<FeatureArea>/`. No mapping library. Entity →
response goes through static `ToResponse()` extension methods grouped in
`Contracts/<FeatureArea>/<FeatureArea>Mappings.cs`; request → entity is written inline in
the controller through the domain factory/method (no `ToDomain()` mapper), because
`userId` comes from the token rather than the DTO. `Domain` entities are never returned
directly.

## 2. Endpoints

All endpoints require authentication except the two marked *anonymous*.

### 2.1 Users

| Method | Path | Purpose |
|---|---|---|
| POST | `/api/auth/register` | *anonymous* — create a `User` (name, email, password), return a JWT |
| POST | `/api/auth/login` | *anonymous* — exchange email + password for a JWT (401 on failure) |
| GET | `/api/users/me` | current user's profile |
| PUT | `/api/users/me` | update the current user's name / email |

### 2.2 Workouts

`Exercise` — shared catalog, not user-scoped:

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/exercises` | list the exercise catalog (optional `muscleGroup` filter) |
| GET | `/api/exercises/{id}` | single catalog exercise |
| POST | `/api/exercises` | add an exercise to the catalog |
| PUT | `/api/exercises/{id}` | update a catalog exercise |
| DELETE | `/api/exercises/{id}` | remove a catalog exercise |

`WorkoutPlan` / `WorkoutPlanExercise` — owned by the current user:

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/workout-plans` | current user's plans (without children) |
| GET | `/api/workout-plans/{id}` | one plan including its exercises |
| POST | `/api/workout-plans` | create a plan (name) |
| PUT | `/api/workout-plans/{id}` | rename a plan |
| DELETE | `/api/workout-plans/{id}` | delete a plan (historical `WorkoutLog` rows survive) |
| POST | `/api/workout-plans/{planId}/exercises` | add an exercise to the plan via `WorkoutPlan.AddExercise` |
| DELETE | `/api/workout-plans/{planId}/exercises/{planExerciseId}` | remove a plan exercise (child's own id) |

### 2.3 Nutrition

`Food` — shared catalog, not user-scoped:

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/foods` | list the food catalog |
| GET | `/api/foods/{id}` | single catalog food |
| POST | `/api/foods` | add a food to the catalog |
| PUT | `/api/foods/{id}` | update a catalog food |
| DELETE | `/api/foods/{id}` | remove a catalog food |

`MealPlan` / `MealPlanEntry` — owned by the current user:

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/meal-plans` | current user's meal plans (without children) |
| GET | `/api/meal-plans/{id}` | one meal plan including its entries |
| POST | `/api/meal-plans` | create a meal plan (name) |
| PUT | `/api/meal-plans/{id}` | rename a meal plan |
| DELETE | `/api/meal-plans/{id}` | delete a meal plan |
| POST | `/api/meal-plans/{planId}/entries` | add a food entry via `MealPlan.AddEntry` |
| DELETE | `/api/meal-plans/{planId}/entries/{entryId}` | remove a meal plan entry (child's own id) |

### 2.4 Progress

Append-only plus delete; all list endpoints take optional `from` / `to` (inclusive,
ISO-8601 UTC) query parameters.

`WorkoutLog` / `WorkoutLogEntry`:

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/workout-logs?from=&to=` | current user's workout logs in a date range |
| GET | `/api/workout-logs/{id}` | one log including its entries |
| POST | `/api/workout-logs` | create a log via `WorkoutLog.Create` (optional `workoutPlanId`) |
| DELETE | `/api/workout-logs/{id}` | delete a log and its entries |
| POST | `/api/workout-logs/{logId}/entries` | add a performed set via `WorkoutLog.AddEntry` |
| DELETE | `/api/workout-logs/{logId}/entries/{entryId}` | remove a log entry |

`MealLog`:

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/meal-logs?from=&to=` | current user's consumed meals in a date range |
| GET | `/api/meal-logs/{id}` | one meal log |
| POST | `/api/meal-logs` | record a consumed food via `MealLog.Create` |
| DELETE | `/api/meal-logs/{id}` | delete a meal log |

`BodyMetricEntry`:

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/body-metrics?from=&to=` | current user's body metrics in a date range |
| GET | `/api/body-metrics/{id}` | one body metric entry |
| POST | `/api/body-metrics` | record weight / body-fat via `BodyMetricEntry.Create` |
| DELETE | `/api/body-metrics/{id}` | delete a body metric entry |

## 3. Auth

**JWT bearer tokens**, hand-rolled on `Microsoft.AspNetCore.Authentication.JwtBearer` —
**not** ASP.NET Core Identity (only its `PasswordHasher<User>` is borrowed, registered in
DI, for PBKDF2 hashing).

- `User` carries a single credential field, `PasswordHash` (`string`, required); `Email`
  is the login identifier and has a unique index. No roles, no security stamp, no
  lockout, no refresh-token table.
- `POST /api/auth/register` and `POST /api/auth/login` are the only `[AllowAnonymous]`
  endpoints; both return a token. Wrong credentials → `401` returned directly by the
  action (indistinguishable from "unknown email"); a duplicate email on register →
  `ValidationException` → `400`.
- Token: HS256, claims `sub` = `User.Id` and `email`, 7-day expiry, no refresh (expiry
  means logging in again). `Jwt:Key` / `Jwt:Issuer` / `Jwt:Audience` come from
  configuration; the key lives in user-secrets locally and is never committed.
- `Program.cs` sets a **fallback authorization policy** requiring an authenticated user,
  so everything is protected by default and anonymous access is opt-in per action.
- Controllers resolve the current user with `User.GetUserId()` — an extension on
  `ClaimsPrincipal` in `src/FitLifePlanner.Api/Common/ClaimsPrincipalExtensions.cs`
  reading `ClaimTypes.NameIdentifier`, throwing `InvalidOperationException` if absent
  (a pipeline misconfiguration, not a client error). No ambient current-user service.
- `FitLifePlanner.Web` sends `Authorization: Bearer <token>` from its typed API clients —
  attached by a single `DelegatingHandler`, token kept in browser `localStorage`
  (ADR-0004). Because the SPA is a separate origin, `Api` enables a CORS policy for the
  origins in `Cors:AllowedOrigins` (no `AllowCredentials` — the token is a header).
