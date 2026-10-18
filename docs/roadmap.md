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
| ~~Exercise catalog search/filter by muscle group~~ | Done — `Exercises.razor` filters the already-loaded list by muscle group as you type (case-insensitive substring), no button and no extra API call | Fast (client-side filter over already-loaded data, no new endpoint) |
| ~~Live macro/calorie running total while building a meal plan~~ | Done (2026-10-01, commit `42b31b9`) — `MealPlanDetail` shows a running macro/calorie preview while composing entries, before saving. This row was stale (still listed as open) until corrected 2026-10-07. | Fast (Web-only computed display, reuses existing `Food` fields) |
| ~~Demo seed command/script for recruiter walkthroughs~~ | Done — `DemoDataSeeder` (`dotnet run --project src/FitLifePlanner.Api -- --seed-demo`), idempotent, seeds a demo login (`demo@fitlifeplanner.local` / `Demo1234!`), 5 exercises, 5 foods, a WorkoutPlan, a MealPlan, and ~2 weeks of workout/meal/body-metric logs with a mild progression trend. Verified end-to-end against a scratch SQLite db: login, `/api/workout-plans`, `/api/exercises`, `/api/body-metrics` all returned the seeded data. | Fast/Normal (a seed script or `dotnet run --seed-demo` flag populating a few sample entities; no schema change) |
| ~~Account settings page~~ | Change-password slice done (2026-10-10) — `PUT /api/users/me/password`, `AccountSettings.razor` at `/account`, linked from `NavMenu.razor`. Nutrition target / body goals editing added to the same page (2026-10-11), reusing the existing `UsersApiClient.UpdateNutritionTargetAsync`/`UpdateGoalsAsync` and the same field shapes as the inline forms. The inline forms on Meal Logs / Body Metrics are deliberately NOT removed — they drive those pages' own live "vs. target"/"progress to goal" displays, so this is purely additive (edit from either place), not a move. | Normal (new page + new password-change endpoint touching auth indirectly; consolidating the two existing inline forms is Fast on its own) |
| ~~Mark favorite exercises / foods for quicker plan building~~ | Done — additive `IsFavorite` bool on `Exercise`/`Food` (SQLite + Postgres migrations), `PUT /api/{exercises,foods}/{id}/favorite`, and a star toggle on `Exercises.razor`/`Foods.razor` that sorts favorites first. Surfacing favorites in `WorkoutPlanDetail`/`MealPlanDetail`'s "add exercise/food" picker (2026-10-10) closes the remaining gap: same `OrderByDescending(IsFavorite).ThenBy(Name)` ordering, with a ★ prefix on favorite options in the `<InputSelect>`. | Fast (one additive boolean field per catalog entity + a toggle in the list UI, no migration risk beyond one new column) |
| ~~Live planned-volume total while building a workout plan~~ | Done (2026-10-13) — `WorkoutPlanDetail` gets the same "plan totals" footer row + "this entry adds: ..." pending preview `MealPlanDetail` already had for macros (item above, 2026-10-01), using the same sets × reps × weight formula `ProgressMappings.TotalVolume` applies to logged workouts, just over planned targets instead of completed sets. Web-only, no API/Domain change. | Fast (Web-only computed display mirroring an existing sibling page's pattern) |
| ~~"Duplicate this plan" for meal plans~~ | Done (2026-10-15) — `WorkoutPlans.razor` got `POST /api/workout-plans/{id}/duplicate` back on 2026-09-25 (ETAP 8+ row above); `MealPlans.razor` never got the sibling endpoint, so a user could only duplicate a workout plan, not a meal plan. Added `POST /api/meal-plans/{id}/duplicate` (`NutritionController`, same clone-via-`AddEntry` pattern as the workout-plan handler) + a "Duplicate" button on `MealPlans.razor`. 2 new integration tests mirroring `DuplicateWorkoutPlan_*`, 107/107 passing. | Fast (mirrors an existing endpoint+button 1:1, no new business rule) |
| ~~Name filter on the Foods catalog page~~ | Done (2026-10-16) — `Exercises.razor` has had a client-side "Filter by muscle group" text filter since the ETAP 8+ exercise-search row above; `Foods.razor` never got an equivalent, even though it's the same shape of catalog page with the same favorites-first list. Added a "Filter by name" input filtering `VisibleFoods` by substring (case-insensitive), same pattern as `Exercises.razor`'s filter. | Fast (client-side filter over already-loaded data, no new endpoint) |
| ~~Favorites-first ordering on `WorkoutLogDetail`'s exercise picker~~ | Done (2026-10-16) — `WorkoutPlanDetail`'s exercise picker got favorites-first ordering + a ★ prefix (2026-10-10 row above), but `WorkoutLogDetail`'s "Add entry" exercise picker (same `ExerciseResponse` catalog) never got it, so logging a workout still meant scrolling the full unsorted catalog. Added `ExerciseCatalogForPicker` (`OrderByDescending(IsFavorite).ThenBy(Name)`) + the ★ prefix, same as `WorkoutPlanDetail`. | Fast (reuses an existing ordering/prefix pattern on one more picker, no new field) |
| ~~Favorites-first ordering on `MealLogs`' food picker~~ | Done (2026-10-16) — same gap as above, one layer over: `MealPlanDetail`'s food picker got favorites-first ordering, but `MealLogs`' "New meal log" food picker (same `FoodResponse` catalog) never did. Added `FoodCatalogForPicker` + the ★ prefix, same as `MealPlanDetail`. | Fast (reuses an existing ordering/prefix pattern on one more picker, no new field) |
| ~~"Log totals" footer on `WorkoutLogDetail`~~ | Done (2026-10-18) — `WorkoutPlanDetail` got a "Plan totals" footer row over sets × reps × weight (2026-10-13 row above), and `WorkoutLogResponse.TotalVolume` already computes the same aggregate for completed entries (used by the Workout Logs trend chart) — but `WorkoutLogDetail`, the page that actually edits a log's entries, never showed that total for the log it's displaying. Added `LogTotalVolume` (Web-only, same formula) + a "Log totals" `<tfoot>` row, same shape as `WorkoutPlanDetail`'s. | Fast (Web-only computed display mirroring an existing sibling page's pattern) |
