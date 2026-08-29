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
