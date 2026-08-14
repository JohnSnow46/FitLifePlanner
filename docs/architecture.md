# Architecture

## Index

- High-level overview → §1
- Layers / modules → §2
- Key architectural decisions → §3 (see `docs/decisions.md` for the full ADR log)

## 1. High-level overview

FitLife Planner is a web app: a Blazor WebAssembly SPA frontend calling an ASP.NET Core
Web API backend over HTTP/JSON. Both target **.NET 10 (LTS)**. Three feature areas at
MVP scope: workout planning, meal/nutrition planning, progress tracking (see
`docs/database.md` §2 for the domain model). Hosting is not yet decided — the app is
designed to run entirely locally for now (SQLite file DB, `dotnet run` for both
projects); no deployment infrastructure exists yet. See ADR-0001.

## 2. Layers / modules

Four projects, dependency direction one-way (no cycles):

```
FitLifePlanner.Web (Blazor WASM SPA)
        |  HTTP/JSON only — no project reference
        v
FitLifePlanner.Api  --->  FitLifePlanner.Infrastructure  --->  FitLifePlanner.Domain
```

- **`FitLifePlanner.Domain`** — entities, enums, business rules. No outward
  dependencies.
- **`FitLifePlanner.Infrastructure`** — EF Core `DbContext`, entity configurations,
  migrations. Depends on `Domain`.
- **`FitLifePlanner.Api`** — controllers/endpoints, DI composition root, DTOs, and thin
  orchestration logic (no separate Application/CQRS layer at this scale). Depends on
  `Domain` + `Infrastructure`.
- **`FitLifePlanner.Web`** — Blazor WebAssembly SPA, consumes `Api` over HTTP only.

No generic repository/unit-of-work abstraction — `Api`/`Infrastructure` use EF Core's
`DbContext` directly. Rationale and alternatives considered: ADR-0001.

## 3. Key architectural decisions

- **ADR-0001** — Backend (ASP.NET Core Web API) + frontend (Blazor WebAssembly) stack,
  and the four-project layering above. See `docs/adr/ADR-0001-architecture-and-stack.md`.
- **ADR-0002** — Data storage: EF Core + SQLite for local dev, production provider
  deferred to the hosting decision. See `docs/adr/ADR-0002-data-storage.md`.
- **ADR-0003** — API conventions, error mapping and JWT auth for `Api`. See
  `docs/api.md` and `docs/adr/ADR-0003-api-conventions-and-auth.md`.
- **ADR-0004** — `Web` internals: typed API clients + bearer `DelegatingHandler`, JWT in
  `localStorage` behind a custom `AuthenticationStateProvider`, own `Contracts` records
  (no shared project), no state-management library; `Api` gains a CORS policy. See
  `docs/adr/ADR-0004-web-frontend-structure-and-auth.md`.
