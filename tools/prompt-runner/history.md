# Prompt history

Permanent, git-tracked record of every prompt batch `prompt-engineer` has generated for
`tools/prompt-runner`. `queue.txt` only ever holds the *current* batch (overwritten on
each new run and cleared once `run.sh` consumes it) — this file is the append-only log
so nothing is lost. Newest batch at the bottom.

## ETAP 1 scaffolding — solution + entities + EF Core + tests (4 prompts)

```
FitLife Planner — ETAP 1 scaffolding, step 1/4: solution + project skeleton.

Repo: FitLifePlanner (ASP.NET Core Web API + Blazor WebAssembly, .NET 10). Fresh session,
no prior context — read `CLAUDE.md` (repo root) for working conventions, then
`docs/adr/ADR-0001-architecture-and-stack.md` for the exact four-project layout this task
implements. Confirm `dotnet --version` reports a .NET 10 SDK before starting; if it's not
installed, stop and report that instead of downgrading the target framework.

Task (mechanical execution of an already-accepted decision — no new architecture call
needed, so skip `architect`/`architect-lite`; dispatch straight to `builder`, then
`reviewer-lite` before calling it done, per `CLAUDE.md`'s mode table):

Scaffold the solution skeleton — four empty-but-compiling .NET 10 projects, correctly
wired, no business code yet:

1. From the repo root: `dotnet new sln -n FitLifePlanner`.
2. Create projects (verify each `.csproj` targets `net10.0`):
   - `dotnet new classlib -n FitLifePlanner.Domain -o src/FitLifePlanner.Domain` — delete
     the generated `Class1.cs`.
   - `dotnet new classlib -n FitLifePlanner.Infrastructure -o src/FitLifePlanner.Infrastructure`
     — delete the generated `Class1.cs`.
   - `dotnet new webapi -n FitLifePlanner.Api -o src/FitLifePlanner.Api --use-controllers`
     — delete the generated `WeatherForecastController.cs` and `WeatherForecast.cs`
     sample files.
   - `dotnet new blazorwasm -n FitLifePlanner.Web -o src/FitLifePlanner.Web` (standalone —
     do NOT use `--hosted`; `Web` must have zero project reference to `Api`, HTTP only,
     per ADR-0001). Leave the default template pages as-is — trimming/styling the UI is
     out of scope here.
3. Project references: `Infrastructure` → `Domain`; `Api` → `Domain` and `Api` →
   `Infrastructure`. `Web` gets no project reference to any backend project.
4. `dotnet sln FitLifePlanner.sln add` all four projects.
5. Do NOT add EF Core packages, a `DbContext`, or any domain entities yet — those are
   later steps in this sequence. This step is scaffolding only.
6. Verify: `dotnet restore` then `dotnet build` from the repo root succeeds with zero
   errors.

When builder is done, have `reviewer-lite` confirm the build passes (no tests exist yet,
so skip the test check) and check nothing beyond the scaffolding above was added. If
reviewer-lite says ready to merge, invoke the `commit` skill (type `chore:` —
repo/tooling scaffolding, not a feature).
```

```
FitLife Planner — ETAP 1 scaffolding, step 2/4: domain entities.

Fresh session, no prior context. Read `CLAUDE.md`, then `docs/database.md` §2 (Schema /
entities) for the authoritative field list, and
`.claude/skills/project-conventions/SKILL.md` for naming/folder rules. Precondition: step
1 already ran — `src/FitLifePlanner.Domain` exists and is part of the solution (if it
doesn't, stop and report that step 1 needs to run first, don't improvise the solution
scaffolding here).

Task (mechanical — skip `architect`/`architect-lite`, straight to `builder` then
`reviewer-lite`):

Implement the MVP domain entities from `docs/database.md` §2 as plain C# classes in
`FitLifePlanner.Domain`, one file per entity, in feature-area folders (`Users`,
`Workouts`, `Nutrition`, `Progress`) per `project-conventions`:

- `Users/User` — Id, Name, Email.
- `Workouts/Exercise`, `Workouts/WorkoutPlan`, `Workouts/WorkoutPlanExercise` (join, with
  Order/TargetSets/TargetReps/TargetWeight).
- `Nutrition/Food`, `Nutrition/MealPlan`, `Nutrition/MealPlanEntry` (join, with a
  `MealType` enum: Breakfast/Lunch/Dinner/Snack, plus DayOfWeek).
- `Progress/WorkoutLog` (+ child `WorkoutLogEntry`), `Progress/MealLog`,
  `Progress/BodyMetricEntry`.

Rules:
- Plain POCOs — properties only, no EF Core attributes/annotations, no `[Table]`/`[Key]`
  etc. (persistence mapping lives in `Infrastructure`, a later step — `Domain` stays
  persistence-ignorant per ADR-0001).
- `Domain` must not reference `Infrastructure`, `Api`, or any EF Core/ASP.NET package —
  it's a plain classlib with zero outward dependencies.
- Nullable FKs stay nullable in code (e.g. `WorkoutLog.WorkoutPlanId` is nullable —
  ad-hoc logging without a plan, and history must survive plan deletion, per
  `docs/database.md` §2 'Key rules').
- No business-rule logic beyond simple property validation/guards yet (full behavior is
  a later ETAP, not this task) — this step is shaping the model, not implementing rules.

After implementing: `dotnet build` from the repo root must still succeed with zero
errors. Have `reviewer-lite` confirm the build passes and spot-check that no EF
Core/Infrastructure dependency leaked into `Domain`. If ready to merge, invoke the
`commit` skill (`feat:` — adds the domain model).
```

```
FitLife Planner — ETAP 1 scaffolding, step 3/4: EF Core + SQLite wiring, initial
migration.

Fresh session, no prior context. Read `CLAUDE.md`, `docs/adr/ADR-0002-data-storage.md`,
and `docs/database.md` (§2 for entities/rules, §3 for the exact migration commands).
Precondition: steps 1-2 already ran — the four projects exist and
`FitLifePlanner.Domain` has the entities from `docs/database.md` §2. If either isn't
true, stop and report it instead of improvising.

Task (mechanical execution of ADR-0002 — skip `architect`/`architect-lite`, straight to
`builder` then `reviewer-lite`; this is an additive-only migration on an empty database,
so fast mode applies even though it touches two projects):

1. Add packages: `Microsoft.EntityFrameworkCore.Sqlite` to `FitLifePlanner.Infrastructure`;
   `Microsoft.EntityFrameworkCore.Design` to `FitLifePlanner.Api` (the startup project
   for EF tooling). Confirm the `dotnet-ef` CLI is available (`dotnet ef --version`); if
   missing, `dotnet tool install --global dotnet-ef`.
2. In `FitLifePlanner.Infrastructure`, add `FitLifePlannerDbContext : DbContext` with a
   `DbSet<T>` per entity from step 2, plus one `IEntityTypeConfiguration<T>` class per
   entity under `Configurations/` (per `project-conventions`), applied via
   `modelBuilder.ApplyConfigurationsFromAssembly(...)`.
3. Configuration must encode the rules from `docs/database.md` §2 'Key rules':
   `WorkoutLog.WorkoutPlanId` is a nullable FK with `DeleteBehavior.SetNull` (or no
   cascade) — deleting a `WorkoutPlan` must NOT delete its historical `WorkoutLog` rows.
   `Exercise`/`Food` are shared/global (no `UserId`).
4. In `FitLifePlanner.Api`'s composition root, register `FitLifePlannerDbContext` for DI
   using `UseSqlite(...)` and a connection string read from configuration. Add
   `"ConnectionStrings": { "Default": "Data Source=fitlifeplanner.db" }` to
   `appsettings.json`/`appsettings.Development.json`. Add `*.db` to the repo's
   `.gitignore` if it isn't already covered (the DB file is git-ignored per ADR-0002).
5. Generate and apply the initial migration exactly as documented in `docs/database.md`
   §3 / `CLAUDE.md` Commands:
   `dotnet ef migrations add InitialCreate --project src/FitLifePlanner.Infrastructure --startup-project src/FitLifePlanner.Api`,
   then
   `dotnet ef database update --project src/FitLifePlanner.Infrastructure --startup-project src/FitLifePlanner.Api`.
   Confirm the `.db` file gets created.
6. `dotnet build` must still succeed with zero errors.

Have `reviewer-lite` confirm the build passes and the migration applied cleanly (command
output, not assumption — see `verification-before-completion`). If ready to merge,
invoke the `commit` skill (`feat:` — EF Core/SQLite wiring + initial migration).
```

```
FitLife Planner — ETAP 1 scaffolding, step 4/4: test project + end-to-end verification.

Fresh session, no prior context. Read `CLAUDE.md` and
`.claude/skills/writing-tests/SKILL.md`. Precondition: steps 1-3 already ran — the four
projects exist, entities exist, `FitLifePlannerDbContext` is wired with an applied
`InitialCreate` migration. If any of that isn't true, stop and report it instead of
improvising.

Task (mechanical — skip `architect`/`architect-lite`, straight to `builder` then
`reviewer-lite`):

1. Create the test project: `dotnet new xunit -n FitLifePlanner.Tests -o tests/FitLifePlanner.Tests`,
   add it to `FitLifePlanner.sln`, and add project references to `FitLifePlanner.Domain`
   and `FitLifePlanner.Infrastructure`.
2. Write one smoke test confirming the EF Core wiring from step 3 actually works:
   `tests/FitLifePlanner.Tests/Infrastructure/FitLifePlannerDbContextTests.cs` — open an
   in-memory SQLite connection explicitly (`new SqliteConnection("Data Source=:memory:")`,
   `.Open()`, pass it to `UseSqlite(connection)`; a bare `:memory:` connection string
   drops the DB when the connection object is closed, so the test must hold one open
   connection for its lifetime), call `context.Database.EnsureCreated()` (or apply
   migrations), then assert you can add and read back one row of the simplest entity
   (e.g. a `User`).
3. Run the full verification sequence and report actual command output for each (per
   `verification-before-completion` — no claim without the command run this turn):
   - `dotnet restore`
   - `dotnet build`
   - `dotnet test` (per `CLAUDE.md` Commands / `writing-tests`)
   - `dotnet run --project src/FitLifePlanner.Api` — confirm it starts and listens, then
     stop it.
   - `dotnet run --project src/FitLifePlanner.Web` — confirm it starts and serves, then
     stop it.
4. If anything in step 3 fails, fix it (this is still `builder`'s job) and re-run until
   all five pass.

Have `reviewer-lite` confirm all five commands' output (build/test/both run checks)
rather than re-running them from scratch, and confirm the new test follows
`writing-tests` conventions. If ready to merge, invoke the `commit` skill (`test:` —
adds the test project and end-to-end scaffolding verification).
```

## Post-ETAP 1 docs sync + ETAP 3 business rules (Workouts/Nutrition/Progress) — 4 prompts

```
FitLife Planner — post-ETAP 1 docs sync: roadmap + CLAUDE.md status (fast mode, docs only).

Fresh session, no prior context. Read `CLAUDE.md` (repo root, "Current status" section),
`docs/roadmap.md`, and `docs/database.md` §2 (Schema/entities). Also run
`git log --oneline -10` and list `src/FitLifePlanner.Domain`,
`src/FitLifePlanner.Infrastructure/Configurations`, and `tests/FitLifePlanner.Tests` to
confirm the actual repo state — don't take the summary below on faith, verify against the
repo first.

Task (docs only, no code changes — fast mode; still run `reviewer-lite` at the end for
consistency with the rest of the workflow, even though there's no build/test to check):

Both `docs/roadmap.md` and `CLAUDE.md`'s "Current status" section still describe ETAP 1
as scaffolding-pending, but four commits already completed it (solution skeleton, 12
domain entities, EF Core+SQLite wiring with the `InitialCreate` migration, and a test
project with a DbContext smoke test) — bring both docs in line with the actual repo
state:

1. `docs/roadmap.md` status table:
   - ETAP 1 row: change status to "✅ Done" and drop the "skeleton scaffolding pending"
     qualifier — it's done, not decided-but-pending.
   - ETAP 2 row ("Data model & database design (`docs/database.md`)"): change status to
     "✅ Done". Add one clause noting the schema is already documented in
     `docs/database.md` §2 and implemented as EF Core configurations in
     `src/FitLifePlanner.Infrastructure/Configurations` — don't restate the schema
     itself, the roadmap only tracks sequencing/status per the file's own header
     comment.
   - Leave the ETAP 3/4/5+ rows as "Planned" — untouched by this task.
2. `CLAUDE.md` "Current status" section: replace the ETAP 1 paragraph's stale claim
   ("No solution/project files exist yet — scaffolding the skeleton ... is the next
   `builder` task") with the actual state: four projects (Domain/Infrastructure/Api/Web)
   scaffolded, 12 domain entities in `Domain`, `FitLifePlannerDbContext` + EF Core
   configurations + applied `InitialCreate` migration in `Infrastructure`, and a
   `FitLifePlanner.Tests` project with a passing DbContext smoke test. State that the
   next step is ETAP 3 (core domain features / business rules on the entities, per
   ADR-0001 — no separate Application layer, rules live on `Domain` entities). Match the
   existing tone/length of that section — this is a factual correction, not an
   expansion into a changelog.
3. Don't touch any other section of `CLAUDE.md` or any other file.

Verification (per `verification-before-completion` — no claim without checking): re-read
both edited files after writing them and confirm the new text doesn't contradict
`docs/decisions.md`'s "## ADR Notes" (which already logged the ETAP 1 steps as done) or
anything in `docs/database.md`.

Have `reviewer-lite` confirm the two edits are accurate against the actual repo state
(entity count, project names, migration name) and that nothing outside
`docs/roadmap.md`/`CLAUDE.md` changed. If ready to merge, invoke the `commit` skill (type
`docs:` — status/roadmap correction, no code or behavior change).
```

```
FitLife Planner — ETAP 3, step 1/3: Workouts business rules (normal mode).

Fresh session, no prior context. Read `CLAUDE.md`,
`docs/adr/ADR-0001-architecture-and-stack.md` (Domain layer has zero outward
dependencies; no separate Application/CQRS layer — business rules live on entities),
`docs/database.md` §2 (schema/key rules), and
`.claude/skills/project-conventions/SKILL.md` (naming, file/folder rules, and the
error-handling convention: `Domain` throws small custom exceptions like
`ValidationException`, never a bare `Exception`). Precondition: the ETAP 1 domain
entities already exist — `src/FitLifePlanner.Domain/Workouts/WorkoutPlan.cs` and
`WorkoutPlanExercise.cs`. If they don't, stop and report that ETAP 1 scaffolding needs to
run first, don't improvise the entities here.

Task (single, simple business rule on existing entities, no schema/migration change —
normal mode: `architect-lite` first for a short plan (goal, files touched, solution,
risks, validation — no ADR, this isn't a new architecture decision), then `builder`, then
`reviewer-lite`):

Add invariant-enforcing behavior to the workout-planning entities in
`FitLifePlanner.Domain/Workouts`:

1. Create `src/FitLifePlanner.Domain/Common/ValidationException.cs` — a plain exception
   type (message constructor, no EF Core/ASP.NET dependency) that `Domain` code throws
   on business-rule violations, per `project-conventions`'s error-handling convention.
   This is the first domain exception in the codebase; later ETAP 3 steps reuse it.
2. On `WorkoutPlan`: add a private backing list of `WorkoutPlanExercise` and a public
   `IReadOnlyCollection<WorkoutPlanExercise> Exercises` navigation property (currently
   `WorkoutPlan` has no way to know which exercises belong to it in memory).
3. Add `WorkoutPlan.AddExercise(int exerciseId, int order, int targetSets, int
   targetReps, decimal targetWeight)`:
   - Throws `ValidationException` if `exerciseId` already appears in `Exercises` for
     this plan (no duplicate exercise within the same plan).
   - Throws `ValidationException` if `order` already appears in `Exercises` for this
     plan (unique `Order` per plan — no two exercises sharing a position).
   - Otherwise creates a `WorkoutPlanExercise` (`WorkoutPlanId = this.Id`) with the
     given values, adds it to the backing list, and returns it.
4. Update `WorkoutPlanExerciseConfiguration.cs`'s existing
   `builder.HasOne<WorkoutPlan>().WithMany()` to `.WithMany(w => w.Exercises)` so it
   matches the new navigation — this only changes how EF Core maps the already-existing
   `WorkoutPlanId` FK column, it must not require a new migration. Confirm this with a
   probe: run
   `dotnet ef migrations add ProbeCheck --project src/FitLifePlanner.Infrastructure --startup-project src/FitLifePlanner.Api`;
   if the generated `Up`/`Down` are non-empty, delete the generated migration files and
   stop to report instead of committing a schema change; if empty, delete the probe
   migration files too (nothing to keep either way).
5. Unit tests in `tests/FitLifePlanner.Tests/Domain/Workouts/WorkoutPlanTests.cs`
   (mirrors the project/feature-area convention): happy path (`AddExercise` succeeds,
   appears in `Exercises`), duplicate-`ExerciseId` throws `ValidationException`,
   duplicate-`Order` throws `ValidationException`.

Don't touch `FitLifePlanner.Api` or any controller — API endpoints are ETAP 4, out of
scope here.

Verification (per `verification-before-completion` — report actual command output, not
assumption):
- `dotnet build` — zero errors.
- `dotnet test --filter "FullyQualifiedName~WorkoutPlanTests"` — all new tests pass.
- Confirm (`git status`) that no stray migration files were left behind by the probe.

Have `reviewer-lite` confirm the build/test output, that `Domain` still has zero outward
dependencies, and that the new exception/tests follow `project-conventions`/
`writing-tests`. If ready to merge, invoke the `commit` skill (`feat:` — Workouts
business rules).
```

```
FitLife Planner — ETAP 3, step 2/3: Nutrition business rules (normal mode).

Fresh session, no prior context. Read `CLAUDE.md`, `docs/database.md` §2, and
`.claude/skills/project-conventions/SKILL.md`. Precondition: step 1/3 (Workouts business
rules) already ran — `src/FitLifePlanner.Domain/Common/ValidationException.cs` exists
and `WorkoutPlan.AddExercise` follows the pattern this step mirrors. If
`ValidationException` doesn't exist yet, stop and report that step 1/3 needs to run
first, don't create a second/divergent exception type here.

Task (single, simple business rule, no schema/migration change — normal mode:
`architect-lite` short plan → `builder` → `reviewer-lite`):

Add the equivalent invariant-enforcing behavior to the meal-planning entities in
`FitLifePlanner.Domain/Nutrition`, mirroring step 1/3's pattern exactly:

1. On `MealPlan`: add a private backing list of `MealPlanEntry` and a public
   `IReadOnlyCollection<MealPlanEntry> Entries` navigation property.
2. Add `MealPlan.AddEntry(int foodId, MealType mealType, decimal quantity, DayOfWeek
   dayOfWeek)`:
   - Throws `ValidationException` if the combination of `foodId` + `mealType` +
     `dayOfWeek` already exists among `Entries` for this plan (no duplicate
     food/meal-slot/day within the same plan).
   - Throws `ValidationException` if `quantity <= 0`.
   - Otherwise creates a `MealPlanEntry` (`MealPlanId = this.Id`) with the given values,
     adds it to the backing list, and returns it.
3. Reuse `FitLifePlanner.Domain.Common.ValidationException` from step 1/3 — don't create
   a new exception type.
4. Update `MealPlanEntryConfiguration.cs`'s existing
   `builder.HasOne<MealPlan>().WithMany()` to `.WithMany(p => p.Entries)`. Same
   migration-probe check as step 1/3: run
   `dotnet ef migrations add ProbeCheck --project src/FitLifePlanner.Infrastructure --startup-project src/FitLifePlanner.Api`,
   confirm `Up`/`Down` are empty, then delete the generated migration files either way
   (if non-empty, stop and report instead of committing a schema change).
5. Unit tests in `tests/FitLifePlanner.Tests/Domain/Nutrition/MealPlanTests.cs`: happy
   path, duplicate-combination throws, `quantity <= 0` throws.

Don't touch `Api`/controllers — out of scope.

Verification (per `verification-before-completion`):
- `dotnet build` — zero errors.
- `dotnet test --filter "FullyQualifiedName~MealPlanTests"` — all new tests pass.
- Confirm (`git status`) no stray migration files were left behind by the probe.

Have `reviewer-lite` confirm build/test output and that this step's `MealPlan.AddEntry`
follows the same shape as step 1/3's `WorkoutPlan.AddExercise` (no divergent pattern for
the same kind of rule). If ready to merge, invoke the `commit` skill (`feat:` —
Nutrition business rules).
```

```
FitLife Planner — ETAP 3, step 3/3: Progress business rules (normal mode).

Fresh session, no prior context. Read `CLAUDE.md`, `docs/database.md` §2, and
`.claude/skills/project-conventions/SKILL.md`. Precondition: steps 1-2/3 already ran —
`src/FitLifePlanner.Domain/Common/ValidationException.cs` exists, and
`WorkoutPlan.AddExercise` / `MealPlan.AddEntry` follow the
private-backing-list-plus-`Add...`-method pattern. If `ValidationException` doesn't
exist yet, stop and report that steps 1-2/3 need to run first, don't improvise.

Task (simple invariants on existing entities, no schema/migration change — normal mode:
`architect-lite` short plan → `builder` → `reviewer-lite`):

Add invariant-enforcing factory methods to the progress-tracking entities in
`FitLifePlanner.Domain/Progress`. Unlike steps 1-2 (which added child-list `Add...`
methods), these entities are dated log records with no duplicate-combination rule — so
the shape here is a static `Create` factory per type, throwing `ValidationException` on
violation. Existing public property setters stay as-is (EF Core materialization needs
them); `Create` is an additive, validated way to construct instances, not a replacement:

1. `WorkoutLog`:
   - Add a private backing list of `WorkoutLogEntry` and a public
     `IReadOnlyCollection<WorkoutLogEntry> Entries`.
   - `static WorkoutLog Create(int userId, DateTime date, string notes, int?
     workoutPlanId)` — throws `ValidationException` if `date` is later than
     `DateTime.UtcNow`.
   - `AddEntry(int exerciseId, int setsCompleted, int repsCompleted, decimal
     weightUsed)` — throws `ValidationException` if `setsCompleted <= 0`,
     `repsCompleted <= 0`, or `weightUsed < 0`; otherwise creates a `WorkoutLogEntry`
     (`WorkoutLogId = this.Id`), adds it, returns it.
2. `MealLog`: `static MealLog Create(int userId, DateTime date, MealType mealType, int
   foodId, decimal quantityConsumed)` — throws `ValidationException` if `date` is in the
   future or `quantityConsumed <= 0`.
3. `BodyMetricEntry`: `static BodyMetricEntry Create(int userId, DateTime date, decimal
   weight, decimal? bodyFatPercent, string notes)` — throws `ValidationException` if
   `date` is in the future or `weight <= 0`. Leave `bodyFatPercent` unvalidated — out of
   scope, no rule was specified for it.
4. Reuse `FitLifePlanner.Domain.Common.ValidationException` — don't create a new
   exception type. Update `WorkoutLogEntryConfiguration.cs`'s existing
   `builder.HasOne<WorkoutLog>().WithMany()` to `.WithMany(l => l.Entries)`. Same
   migration-probe check as steps 1-2/3: run
   `dotnet ef migrations add ProbeCheck --project src/FitLifePlanner.Infrastructure --startup-project src/FitLifePlanner.Api`,
   confirm `Up`/`Down` are empty, then delete the generated migration files either way
   (if non-empty, stop and report instead of committing a schema change).
5. Unit tests, one file per type: `tests/FitLifePlanner.Tests/Domain/Progress/WorkoutLogTests.cs`,
   `MealLogTests.cs`, `BodyMetricEntryTests.cs` — happy path plus one test per invariant
   listed above (future date rejected, non-positive counts/quantity/weight rejected).

Don't touch `Api`/controllers — out of scope. This closes out ETAP 3; don't start ETAP 4
(API layer) work.

Verification (per `verification-before-completion`):
- `dotnet build` — zero errors.
- `dotnet test --filter "FullyQualifiedName~Progress"` — all new tests pass.
- `dotnet test` (full suite) — confirm nothing from steps 1-2/3 regressed.
- Confirm (`git status`) no stray migration files were left behind by the probe.

Have `reviewer-lite` confirm build/test output and that all three `Create` factories
follow the same shape. If ready to merge, invoke the `commit` skill (`feat:` — Progress
business rules, closes ETAP 3).
```
