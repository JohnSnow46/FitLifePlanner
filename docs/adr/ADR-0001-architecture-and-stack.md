# ADR-0001: Solution architecture and technology stack

**Date:** 2026-08-05
**Status:** Accepted

**Context.**
The project owner already fixed: web app (SPA frontend + backend API), C#/.NET for the
backend (and for the frontend if a .NET-native option fits), hosting deferred ("we'll
see later"). Scope is three feature areas at MVP size (workout planning, meal/nutrition
planning, progress tracking) with no further product spec. This is a solo portfolio/
learning project; `CLAUDE.md`'s priority order is working feature > fast iteration >
consistency, explicitly not designing for scale/flexibility it doesn't need. A layering
decision and a frontend framework decision are both needed before any code is written.

**Decision.**
- **Backend:** ASP.NET Core Web API, targeting **.NET 10 (LTS)**, exposing REST
  endpoints.
- **Frontend:** **Blazor WebAssembly** (standalone, .NET 10) as the SPA, calling the API
  over HTTP/JSON. Chosen over a JS/TS SPA framework (React/Angular/etc.) to keep the
  entire stack in one language/toolchain for a solo project — no second package
  ecosystem (npm/node) to install, version, or context-switch into. Blazor WASM (not
  Blazor Server) is used specifically so the frontend ships as static files with no
  server-side circuit/session dependency, keeping the later hosting decision open
  (static host + API, or same host — either works).
- **Backend project layout — four projects, no separate Application/CQRS layer:**
  - `FitLifePlanner.Domain` — entities, enums, business rules/domain logic. No
    dependencies on any other project.
  - `FitLifePlanner.Infrastructure` — EF Core `DbContext`, entity configurations,
    migrations, persistence. Depends on `Domain`.
  - `FitLifePlanner.Api` — ASP.NET Core Web API: controllers/endpoints, DI composition
    root, request/response DTOs, and the thin orchestration logic that a dedicated
    Application layer would otherwise hold. Depends on `Domain` + `Infrastructure`.
  - `FitLifePlanner.Web` — Blazor WebAssembly SPA. Talks to `Api` only over HTTP; no
    project reference to the backend.
- **No generic repository/unit-of-work abstraction.** `Api`/`Infrastructure` use EF
  Core's `DbContext` directly (optionally behind small, feature-shaped query/command
  services if a handler grows unwieldy) — `DbContext` already is a unit-of-work, and
  there is no plan to swap ORMs, so an extra abstraction layer would be ceremony without
  payoff.

**Alternatives considered.**
- *Full Clean Architecture (Domain/Application/Infrastructure/Api, CQRS via MediatR).*
  Rejected for now — three small feature areas at MVP scope don't need pipeline/handler
  ceremony; it would slow iteration without a corresponding benefit. The `Domain`/
  `Infrastructure` split already in place makes introducing an `Application` layer later
  a low-cost, additive change if the domain grows.
- *JS/TS SPA (React/Angular) + .NET API.* Rejected — would add a second toolchain for no
  clear benefit on a solo .NET portfolio project.

**Consequences.**
- Business/orchestration logic that doesn't belong on an entity lives in `Api` endpoint
  handlers (or small services within `Api`) for now, not a separate layer.
- `Web` and `Api` are fully decoupled (HTTP only), so they can be hosted separately or
  together without redesign once hosting is decided.
- If a feature area's logic outgrows "thin handler in `Api`," extracting an `Application`
  project is a follow-up ADR and a mechanical move, not a redesign — `Domain` has no
  outward dependencies to unwind.
- Reversal cost of the overall shape (e.g. moving to Clean Architecture, or replacing
  Blazor with a JS SPA) is moderate: project references and the `Web` project change,
  `Domain`/`Infrastructure` are largely unaffected.
