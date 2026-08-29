# FitLife Planner

[![CI](https://github.com/JohnSnow46/FitLifePlanner/actions/workflows/ci.yml/badge.svg)](https://github.com/JohnSnow46/FitLifePlanner/actions/workflows/ci.yml)

A personal fitness and lifestyle planning application — workout planning, meal/nutrition
planning, and progress tracking in one place. Built as a learning/portfolio project,
developed iteratively with [Claude Code](https://claude.com/claude-code).

> **Status:** ETAP 0-6 complete — workout, nutrition, and progress tracking are fully
> usable end to end (API + Blazor UI, JWT auth). ETAP 7 (delivery: hosting, production
> DB, CI, demo polish) is in progress — CI is set up (GitHub Actions runs restore/build/
> format/test on every push and PR to `develop`/`main`); hosting (Render) and the
> production database (PostgreSQL) are decided and implemented, see
> [ADR-0005](docs/adr/ADR-0005-hosting-and-production-database.md) and
> [docs/deployment.md](docs/deployment.md); demo polish (screenshots) is still open — see
> [Roadmap](#roadmap) below.

## Screenshot

_Coming soon._

## Tech stack

ASP.NET Core Web API + Blazor WebAssembly SPA, both on .NET 10. EF Core + SQLite for
local dev. See [docs/architecture.md](docs/architecture.md), [docs/database.md](docs/database.md),
[docs/api.md](docs/api.md), and ADR-0001/ADR-0002 in [docs/decisions.md](docs/decisions.md)
for the full rationale.

## Local requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Getting started

```
dotnet restore
dotnet ef database update --project src/FitLifePlanner.Infrastructure --startup-project src/FitLifePlanner.Api
dotnet run --project src/FitLifePlanner.Api
dotnet run --project src/FitLifePlanner.Web
```

Run the test suite with `dotnet test`. Full command reference in [CLAUDE.md](CLAUDE.md)
and [docs/development.md](docs/development.md).

### Run with Docker

```
cp .env.example .env
# edit .env and set JWT_KEY to a long random secret
docker compose up --build
```

API is served at `http://localhost:8080`, the Blazor WebAssembly frontend at
`http://localhost:8081`. The SQLite database file persists across restarts on a named
Docker volume.

## Documentation

| Area | File |
|---|---|
| Architecture | [docs/architecture.md](docs/architecture.md) |
| Database | [docs/database.md](docs/database.md) |
| API | [docs/api.md](docs/api.md) |
| Architectural decisions (ADRs) | [docs/decisions.md](docs/decisions.md) |
| Deployment (Render) | [docs/deployment.md](docs/deployment.md) |
| Roadmap | [docs/roadmap.md](docs/roadmap.md) |
| Development workflow (branches, commits, PRs) | [docs/development.md](docs/development.md) |

This project is set up to work with Claude Code — see [CLAUDE.md](CLAUDE.md) for the
project brief it reads, and the [Claude Code setup](#claude-code-setup) section below for
the agent/skill pipeline.

## Roadmap

| Stage | Goal | Status |
|---|---|---|
| ETAP 0 | Repo, Claude Code setup, base documentation, git workflow | ✅ Done |
| ETAP 1 | Architecture analysis: stack choice, module/layer design, first ADRs | ✅ Done |
| ETAP 2 | Data model & database design | ✅ Done |
| ETAP 3 | Core domain features | ✅ Done |
| ETAP 4 | API layer | ✅ Done |
| ETAP 5 | Frontend foundation & auth | ✅ Done |
| ETAP 6 | Feature UI (Workouts, Nutrition, Progress, dashboard) | ✅ Done |
| ETAP 7 | Delivery: hosting, production DB, CI, demo polish | 🔄 In progress — CI done |

Full detail in [docs/roadmap.md](docs/roadmap.md).

## Git workflow

```
feature/*  →  Pull Request  →  Code Review  →  merge into develop  →  release into main
```

Commits follow [Conventional Commits](https://www.conventionalcommits.org/):
`feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`. Full detail in
[docs/development.md](docs/development.md).

## Claude Code setup

This repo is adapted from
[claude-code-project-template](https://github.com/JohnSnow46/claude-code-project-template):
a cost-tiered agent pipeline (`.claude/agents/`) and an ADR-backed decision log
(`docs/decisions.md` + `docs/adr/`). See [CLAUDE.md](CLAUDE.md) for the work-mode
classification (fast/normal/deep) and how docs are read.

This repo also has the [dotnet-skills](https://github.com/Aaronontheweb/dotnet-skills)
plugin installed (user scope) for additional .NET-specific skills on top of the
template's generic ones.

## License

[MIT](LICENSE)
