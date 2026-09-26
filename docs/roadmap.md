# Roadmap

Staged plan for FitLife Planner. Each stage's detailed output lives in the docs it
produces (architecture, database, api) rather than being duplicated here — this file
tracks sequencing and status only.

| Stage | Goal | Status |
|---|---|---|
| ETAP 0 | Repo, Claude Code setup, base documentation, git workflow | ✅ Done |
| ETAP 1 | Architecture analysis: stack choice, module/layer design, first ADRs | ✅ Done (ADR-0001, ADR-0002) |
| ETAP 2 | Data model & database design (`docs/database.md`) | ✅ Done — schema documented in `docs/database.md` §2, implemented as EF Core configurations in `src/FitLifePlanner.Infrastructure/Configurations` |
| ETAP 3 | Core domain features (MVP scope TBD after ETAP 1) | ✅ Done — business rules on `WorkoutPlan`/`MealPlan`/`WorkoutLog`/`MealLog`/`BodyMetricEntry` (Domain layer, ADR-0001), unit-tested |
| ETAP 4 | API layer (`docs/api.md`) | ✅ Done — `UsersController` (JWT auth), `WorkoutsController`, `NutritionController`, and the `Progress` controllers (`WorkoutLogsController`/`MealLogsController`/`BodyMetricEntriesController`) implemented per ADR-0003 |
| ETAP 5 | Frontend foundation & auth in `FitLifePlanner.Web` (typed API clients, JWT auth, protected routing) | ✅ Done (ADR-0004) |
| ETAP 6 | Feature UI on top of that foundation: Workouts, Nutrition, Progress pages, dashboard | ✅ Done (ADR-0004) |
| ETAP 7 | Delivery: hosting + production DB provider (deferred by ADR-0002), CI, README/demo polish | 🔄 In progress — CI done; hosting + production DB decided (ADR-0005) and implemented (`docs/deployment.md`); demo polish (screenshots) still open |

No business features are implemented before ETAP 1 is complete — see `CLAUDE.md`
"Current status" for the authoritative current stage.

## ETAP 8+ (post-MVP extension ideas)

Not committed to, not sequenced — candidates for after ETAP 7 closes, each sized against
the fast/normal/deep table in `CLAUDE.md`. Picking one still starts with `## ADR Notes`
in `docs/decisions.md`, not this list.

| Idea | Addresses | Rough size |
|---|---|---|
| Progress charts (body metrics over time, workout volume trend) on the dashboard | `BodyMetricEntry`/`WorkoutLog` history is already logged but only viewable as flat lists — no trend view | Normal (new `Web`-only aggregation + a charting component, no schema change) |
| Weekly nutrition summary (macros logged vs. a per-user target) | `MealLog` entries exist per-meal but nothing aggregates a day/week against a goal | Normal (new read endpoint + aggregation logic + Web page; optional new `NutritionTarget` field is additive) |
| Workout plan templates / "duplicate this plan" | Users building a new `WorkoutPlan` today start from empty — no reuse of a previous plan's exercise list | Fast/Normal (one new domain method cloning `WorkoutPlan` + `AddExercise` entries, one endpoint, one button) |
| ~~Body metric goals (target weight/body fat + progress-to-goal indicator)~~ | Done — `User.SetGoals`, `PUT /api/users/me/goals`, goal section on the Body Metrics page | Fast (single additive field/entity + a computed display value, no migration risk beyond one new column) |
| CSV export of progress logs (workout/meal/body-metric history) | No way to get data out of the app today; a common, low-risk portfolio feature to demonstrate | Fast (read-only endpoint(s) + client-side download, no new dependency) |
| PWA / offline shell for `Web` | Blazor WASM already ships as a static app; installable + cached-shell is a template-level addition, not a new backend | Normal (service worker + manifest wiring; offline *data* sync explicitly out of scope — would be Deep) |
| Reminder to log today's workout/meal (in-app banner, not push/email) | No nudge exists today if a user forgets to log; an external push/email channel is a real new integration, so start with the in-app version | Fast (derive "logged today?" from existing data, show a `Web`-only banner) — a push/email version would be Deep (new external service, ADR-worthy) |

Each row is a candidate ADR + ETAP entry when picked up, not a promise — re-evaluate
against `CLAUDE.md`'s "portfolio project, don't design for scale it doesn't need" rule
before starting any of them.
