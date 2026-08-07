# Database

## Index

- Storage choice → §1
- Schema / entities → §2
- Migrations → §3

## 1. Storage choice

EF Core (Code-First, migrations), SQLite as the local-dev provider. Production provider
is undecided — deferred to the hosting decision. Rationale and alternatives: ADR-0002
(`docs/adr/ADR-0002-data-storage.md`).

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
