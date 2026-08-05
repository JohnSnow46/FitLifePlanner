# FitLife Planner

A personal fitness and lifestyle planning application — workout planning, meal/nutrition
planning, and progress tracking in one place. Built as a learning/portfolio project,
developed iteratively with [Claude Code](https://claude.com/claude-code).

> **Status:** ETAP 0 (repo & environment setup) complete. No business features are
> implemented yet — see [Roadmap](#roadmap) below.

## Screenshot

_Coming soon — no UI yet (ETAP 0 is setup-only)._

## Tech stack

Not yet decided. The stack, architecture, and data model are chosen in **ETAP 1
(architecture analysis)** and documented in [docs/architecture.md](docs/architecture.md),
[docs/database.md](docs/database.md), and [docs/api.md](docs/api.md) once that's done.

## Local requirements

TBD once the stack is chosen (ETAP 1). Will be listed here and in
[docs/development.md](docs/development.md).

## Getting started

TBD once the stack is chosen (ETAP 1). Will be listed here and in
[docs/development.md](docs/development.md).

## Documentation

| Area | File |
|---|---|
| Architecture | [docs/architecture.md](docs/architecture.md) |
| Database | [docs/database.md](docs/database.md) |
| API | [docs/api.md](docs/api.md) |
| Architectural decisions (ADRs) | [docs/decisions.md](docs/decisions.md) |
| Roadmap | [docs/roadmap.md](docs/roadmap.md) |
| Development workflow (branches, commits, PRs) | [docs/development.md](docs/development.md) |

This project is set up to work with Claude Code — see [CLAUDE.md](CLAUDE.md) for the
project brief it reads, and the [Claude Code setup](#claude-code-setup) section below for
the agent/skill pipeline.

## Roadmap

| Stage | Goal | Status |
|---|---|---|
| ETAP 0 | Repo, Claude Code setup, base documentation, git workflow | ✅ Done |
| ETAP 1 | Architecture analysis: stack choice, module/layer design, first ADRs | ⏳ Next |
| ETAP 2+ | Data model, core features, API layer | Planned |

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

## License

[MIT](LICENSE)
