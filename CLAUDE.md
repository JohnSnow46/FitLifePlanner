# FitLife Planner

A personal fitness and lifestyle planning app (workout planning, meal/nutrition
planning, and progress tracking). Backend: ASP.NET Core Web API; frontend: Blazor
WebAssembly SPA; both .NET 10. See `docs/architecture.md` and ADR-0001.

## Docs (don't duplicate their content here)

| Area | File |
|---|---|
| Architecture / layers / modules | `docs/architecture.md` (has `## Index`) |
| Data model / storage | `docs/database.md` (has `## Index`) |
| API / interface layer | `docs/api.md` (has `## Index`) |
| Roadmap / stage tracking | `docs/roadmap.md` |
| Git workflow, commit convention, local dev setup | `docs/development.md` |
| Architectural decisions | `docs/decisions.md` = index → `docs/adr/ADR-NNNN.md` = content; `## ADR Notes` = per-task usage log |

## Work modes (`.claude/agents/`)

Default mode is **fast**. The main thread picks the mode itself from the table below —
no question to the user, and no dedicated classifier agent to spawn. When in doubt, pick
the lower mode and escalate only if an architectural decision genuinely comes up
mid-implementation.

| Signal | Fast (small) | Normal (medium) | Deep (large) |
|---|---|---|---|
| Areas/modules touched | 1 | 2-3 | changes a module boundary or pattern |
| New business/domain rule | none | single, simple | complex / spans multiple entities |
| Schema or migration change | none | single, additive | changes existing schema/relations |
| Security/auth/payments touched | no | indirectly | directly |
| Examples | new page, UI component, styling, simple CRUD endpoint | new module, external integration | large refactor, architecture change, critical decision |

**Workflow per mode:**
- **Fast** (default): `builder` → `reviewer-lite`.
- **Normal**: `architect-lite` → `builder` → `reviewer-lite`.
- **Deep**: `architect` → `builder` → `reviewer`.

`architect`/`reviewer` (full) — deep mode ONLY. `architect-lite` doesn't write ADRs or
touch `docs/decisions.md`/area docs — it gives a short plan (goal, files touched,
solution, risks, validation). `reviewer-lite` checks only what blocks merging
(build/test, security, obvious bugs) — it skips the full architecture/style audit.

Number of areas touched is a **supporting** signal, not sufficient on its own: a simple
CRUD endpoint naturally touches more than one area, and still stays fast mode unless it
introduces a new business rule, a non-additive migration, or touches
security/auth/payments directly. Don't escalate mode just because a change touches more
than one file.

### Mandatory rules

**Default behavior:** small tasks MUST go through `builder` → `reviewer-lite`. Don't
spawn full `architect`/`reviewer` until a task clearly meets the normal/deep criteria
above.

**Scope discipline:** agents deliver working changes, not documentation. Docs (ADRs,
full architect plans) get written only where a real new decision is made and it actually
helps later work — not as a goal in itself.

**Completion conditions:** each agent stops once it reaches its defined output (see its
file in `.claude/agents/`). It doesn't keep exploring unrelated parts of the repo "while
it's there."

**Priority order:** 1) working feature, 2) fast iteration, 3) consistency with existing
code. This is a portfolio/learning project — don't design for scale/flexibility it
doesn't need without an explicit ask.

**Stage-end audit:** when an ETAP is marked done in "Current status", run one full
`reviewer` pass (no `architect` needed) over everything built in that stage before
starting the next ETAP. `reviewer-lite`'s per-task diff scope doesn't reliably catch
cross-controller issues like ownership checks or FK cascade config — see the
2026-08-15 "Post-review data-integrity hardening" entry in `docs/decisions.md`, where
this exact gap shipped through ETAP 4-6 fast-mode reviews unnoticed.

## Using the docs

1. Task in a known area → check `## ADR Notes` in `docs/decisions.md` first.
2. No hit → `## Index` in `docs/decisions.md`, pick specific ADR numbers.
3. Open (or delegate to `architect`) ONLY `docs/adr/ADR-NNNN.md` for the numbers you
   picked — never the whole `docs/adr/` directory, never `docs/decisions.md` in full as
   a way to reach ADR content.
4. Never `Read` a file over ~150 lines in the main thread (area docs, single ADRs)
   without an offset — use its `## Index` + `Read` offset/limit, or delegate a specific
   question to a subagent with a capped response (max ~15 points, no quoting, no
   transcribing). Only a summary comes back to the main thread. Exception: a file you're
   about to edit.
5. Search before reading: grep for the pattern → `Read` with offset/limit. Never `Read`
   without a limit on a file of unknown size.
6. After a task in **normal/deep mode**: the main/orchestrating thread (not any
   subagent), once the whole agent chain finishes (architect/architect-lite → builder →
   reviewer/reviewer-lite), appends an entry to the top of `## ADR Notes` in
   `docs/decisions.md` (template in that file) — ADRs used and their impact on the
   implementation, plus ADRs read-but-unused. In **fast mode**, skip this step if nothing
   from the ADRs was used.
7. Don't ask if you can check: `git log --oneline -20`, `git status`, repo files.
   `AskUserQuestion` only for product decisions that can't be inferred from the repo, max
   one round of questions per session.
8. After research, before implementation: "Research done, ~Nk tokens loaded. Suggest
   `/compact`."

## Global conventions

- Respond in Polish — the project owner communicates in Polish.
- Git workflow, branch naming, and commit convention (`feat:`/`fix:`/`refactor:`/
  `docs:`/`test:`/`chore:`) → `docs/development.md`. Don't invent a different prefix set.
- Naming/typing/error-handling conventions → `.claude/skills/project-conventions/SKILL.md`
  (loaded on demand, not duplicated here).

## Commands

Stack: ASP.NET Core Web API + Blazor WebAssembly, .NET 10; EF Core + SQLite (local dev).
See ADR-0001, ADR-0002.

| Action | Command |
|---|---|
| Restore dependencies | `dotnet restore` |
| Build | `dotnet build` |
| Run backend API | `dotnet run --project src/FitLifePlanner.Api` |
| Run frontend (Blazor WASM) | `dotnet run --project src/FitLifePlanner.Web` |
| Run tests | `dotnet test` |
| Format / lint | `dotnet format --verify-no-changes` |
| Add EF Core migration | `dotnet ef migrations add <Name> --project src/FitLifePlanner.Infrastructure --startup-project src/FitLifePlanner.Api` |
| Apply migrations | `dotnet ef database update --project src/FitLifePlanner.Infrastructure --startup-project src/FitLifePlanner.Api` |

`src/`/`tests/` layout is scaffolded (see "Current status" below and `docs/roadmap.md`).

## Environment

- Edited via the Claude Code VS Code extension.
- Review process: GitHub pull requests (`feature/*` → PR → review → merge into
  `develop` → release into `main`), see `docs/development.md`. Not local-diff review.
- No CI configured yet — add once the stack/test runner exists.

## Current status

**ETAP 0 (repo & environment setup) — done.** Repo, Claude Code config, base docs
structure, git workflow, and branches are in place. No business/domain code exists yet.

**ETAP 1 (architecture analysis) — done.** Stack, layering, and data storage are
decided (ADR-0001, ADR-0002); `docs/architecture.md` and `docs/database.md` are filled
in. `docs/api.md` and `.claude/settings.json` permissions are still open — the API
surface and auth mechanism come with ETAP 4. Skeleton is scaffolded: four projects
(Domain/Infrastructure/Api/Web), 12 domain entities in `Domain`,
`FitLifePlannerDbContext` + EF Core configurations + applied `InitialCreate` migration
in `Infrastructure`, and a `FitLifePlanner.Tests` project with a passing DbContext
smoke test.

**ETAP 3 (core domain features) — done.** Business rules added directly on the
`Domain` entities (ADR-0001, no separate Application layer): `WorkoutPlan.AddExercise`,
`MealPlan.AddEntry`, and `Create`/`AddEntry` factories on `WorkoutLog`/`MealLog`/
`BodyMetricEntry`, all throwing `Domain.Common.ValidationException` on invariant
violations. Unit-tested per entity; no schema/migration change.

**ETAP 4 (API layer) — done.** REST API per ADR-0003 (`docs/api.md`): unversioned
`/api/<kebab-plural>` routes, `NotFoundException`/`ValidationException` → 404/400 via
`GlobalExceptionHandler`, JWT bearer auth (`User.PasswordHash`,
`ClaimsPrincipal.GetUserId()`). Controllers: `UsersController` (register/login/me),
`WorkoutsController` (`Exercise` catalog + owned `WorkoutPlan`), `NutritionController`
(`Food` catalog + owned `MealPlan`), and the `Progress` area split across
`WorkoutLogsController`/`MealLogsController`/`BodyMetricEntriesController` (dated logs,
append-only + delete). MVP API surface is complete.

**ETAP 5 (frontend foundation & auth) — done.** `FitLifePlanner.Web` per ADR-0004:
JWT in `localStorage` behind `TokenStore` + `JwtAuthenticationStateProvider`, bearer
`DelegatingHandler` on typed `<FeatureArea>ApiClient`s, hand-written `Web/Contracts`
records, CORS policy on `Api`. `UsersApiClient` + Login/Register pages, protected
routing, auth-aware `NavMenu`, shared `LoadingIndicator`/`ErrorAlert` components.
Every later frontend stage is mechanical (ADR-0004 Consequences): add
`Contracts/<FeatureArea>` records → add methods to `<FeatureArea>ApiClient` → add pages
under `Pages/<FeatureArea>/`.

**ETAP 6 (feature UI) — done.** Workouts (`Exercises`/`WorkoutPlans`/
`WorkoutPlanDetail`), Nutrition (`Foods`/`MealPlans`/`MealPlanDetail`), Progress
(`WorkoutLogs`/`WorkoutLogDetail`/`MealLogs`/`BodyMetrics`) pages and a read-only
dashboard on `Home.razor` (`<AuthorizeView>`-gated summary + welcome view for
anonymous visitors), all built per ADR-0004's mechanical pattern (typed
`<FeatureArea>ApiClient` + hand-written `Contracts`). Next stage is ETAP 7, see
`docs/roadmap.md`.

---

Keep this file short — every line loads into every conversation, and a bloated
CLAUDE.md causes Claude to ignore half of it. For each line, ask: *"would removing this
cause a mistake?"* If not, cut it.

Rules that only matter sometimes (detailed conventions, one-off workflows) belong in a
skill under `.claude/skills/`, not here — skills load on demand. See
[README.md](README.md) for the full picture of this project's agents and skills.
