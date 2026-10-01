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
| ~~Progress charts (body metrics over time, workout volume trend)~~ | Done — `TrendChart` component (inline SVG, no charting dependency) on the Body Metrics page (weight trend) and Workout Logs page (volume trend, `TotalVolume` added to `WorkoutLogResponse`) | Normal (new `Web`-only aggregation + a charting component, no schema change) |
| ~~Weekly nutrition summary (macros logged vs. a per-user target)~~ | Done — `User.SetNutritionTarget`/`PUT /api/users/me/nutrition-target`, `GET /api/meal-logs/summary` (per-day macro totals, last 7 days by default), target + summary table on the Meal Logs page (2026-09-28) | Normal (new read endpoint + aggregation logic + Web page; optional new `NutritionTarget` field is additive) |
| ~~Workout plan templates / "duplicate this plan"~~ | Done — `POST /api/workout-plans/{id}/duplicate` + "Duplicate" button on the Workout Plans page (2026-09-25, commit `3c97847`). This row was stale (still listed as open) until corrected 2026-09-28. | Fast/Normal (one new domain method cloning `WorkoutPlan` + `AddExercise` entries, one endpoint, one button) |
| ~~Body metric goals (target weight/body fat + progress-to-goal indicator)~~ | Done — `User.SetGoals`, `PUT /api/users/me/goals`, goal section on the Body Metrics page | Fast (single additive field/entity + a computed display value, no migration risk beyond one new column) |
| ~~CSV export of progress logs (workout/meal/body-metric history)~~ | Done — `GET /api/{workout-logs,meal-logs,body-metrics}/export` + "Download CSV" button on each Progress page (Blob download via a small `wwwroot/js/download.js`, no new dependency) | Fast (read-only endpoint(s) + client-side download, no new dependency) |
| ~~PWA / offline shell for `Web`~~ | Done — `manifest.json`, dev-mode no-op `service-worker.js` swapped for `service-worker.published.js` on publish (offline-caches the static shell), registered from `index.html` (2026-09-30, commit `c4b520b`). Offline *data* sync stays out of scope. | Normal (service worker + manifest wiring; offline *data* sync explicitly out of scope — would be Deep) |
| ~~Reminder to log today's workout/meal (in-app banner, not push/email)~~ | Done — dashboard banner derived from the already-loaded 30-day workout/meal log window, no new endpoint | Fast (derive "logged today?" from existing data, show a `Web`-only banner) — a push/email version would be Deep (new external service, ADR-worthy) |

Each row is a candidate ADR + ETAP entry when picked up, not a promise — re-evaluate
against `CLAUDE.md`'s "portfolio project, don't design for scale it doesn't need" rule
before starting any of them.

## ETAP 9+ (proposed 2026-09-30 — all ETAP 8+ rows above are now done)

| Idea | Addresses | Rough size |
|---|---|---|
| Exercise catalog search/filter by muscle group | `Exercise.MuscleGroup` already exists on every row, but `Exercises.razor` shows a flat unfiltered list — grows unwieldy past a handful of entries | Fast (client-side filter over already-loaded data, no new endpoint) |
| Live macro/calorie running total while building a meal plan | `Food` already carries per-100g macros; `MealPlanDetail` only shows the total after saving each entry, not while composing one | Fast (Web-only computed display, reuses existing `Food` fields) |
| Demo seed command/script for recruiter walkthroughs | ETAP 7's "demo polish (README screenshots)" item is still open per `CLAUDE.md` "Current status" — a fresh clone has no data to screenshot or click through | Fast/Normal (a seed script or `dotnet run --seed-demo` flag populating a few sample entities; no schema change) |
| Account settings page (change password, edit nutrition target / body goals in one place) | There's no way to change a password after registration at all; nutrition target is edited inline on the Meal Logs page and goals inline on Body Metrics — no single "my account" screen, and `NavMenu.razor` has no settings/account link | Normal (new page + new password-change endpoint touching auth indirectly; consolidating the two existing inline forms is Fast on its own) |
| Mark favorite exercises / foods for quicker plan building | `Exercises.razor`/`Foods.razor` are flat catalogs with no way to surface the ones actually used often when building a `WorkoutPlan`/`MealPlan` on `WorkoutPlanDetail.razor`/`MealPlanDetail.razor` | Fast (one additive boolean field per catalog entity + a toggle in the list UI, no migration risk beyond one new column) |
