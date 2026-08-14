---
name: project-conventions
description: Detailed coding conventions and architectural patterns for this project. Use when writing new code, to check naming, structure, and patterns.
---

# Project Conventions

Applies to the four-project layout from ADR-0001
(`FitLifePlanner.Domain` → `Infrastructure` → `Api`, `Web`).

## Language conventions
- Nullable reference types are enabled in every project (`<Nullable>enable</Nullable>`).
  Don't use the `!` null-forgiving operator except right after a check that already
  proves non-null.
- One class/interface/enum per file, filename matches the type name (already the
  pattern in `Domain`'s feature-area folders — keep it for `Api`/`Web` too).
- Async methods in `Api`/`Infrastructure` that can be cancelled from an outer scope (HTTP
  request, background job) take and propagate a `CancellationToken` through the call
  chain. `Domain` methods stay synchronous (no EF Core/IO) — this doesn't apply there.

## Naming
- Types/methods/properties: PascalCase. Private fields: `_camelCase`. Parameters/locals:
  camelCase. Async methods always end in `Async` (`AddExerciseAsync`).
- No `I`-prefixed interface unless a real swappable implementation exists — matches
  ADR-0001's "no abstraction without payoff."
- EF Core config classes: `<Entity>Configuration`, implementing `IEntityTypeConfiguration<T>`.
- `Api` DTOs: `<Entity><Verb>Request` / `<Entity>Response` (e.g. `CreateWorkoutPlanRequest`,
  `WorkoutPlanResponse`) — never return a `Domain` entity directly from an endpoint.
- Blazor components: PascalCase `.razor`, one component per file, named for what it
  renders (`WorkoutPlanList.razor`, not `List.razor`).

## File/folder structure
- New entity → `src/FitLifePlanner.Domain/<FeatureArea>/<Entity>.cs` (feature areas:
  `Workouts`, `Nutrition`, `Progress`, shared `Users`).
- New EF Core config → `src/FitLifePlanner.Infrastructure/Configurations/<Entity>Configuration.cs`;
  migrations in `src/FitLifePlanner.Infrastructure/Migrations/` (generated, never hand-edited).
- New endpoint → `src/FitLifePlanner.Api/Controllers/<FeatureArea>Controller.cs`; its
  request/response DTOs → `src/FitLifePlanner.Api/Contracts/<FeatureArea>/`.
- New Blazor page → `src/FitLifePlanner.Web/Pages/<FeatureArea>/`; shared/reusable
  components → `src/FitLifePlanner.Web/Components/`. Calls to `Api` go through a typed
  client in `src/FitLifePlanner.Web/Services/` — never an inline `HttpClient` call in a
  component (`Web` talks to `Api` over HTTP only, ADR-0001).
- Tests mirror the project under test: `tests/FitLifePlanner.Tests/<ProjectName>/<FeatureArea>/<TypeUnderTest>Tests.cs`.

## Error handling
**Exceptions + a global `ProblemDetails` middleware in `Api`** — not a `Result<T>`
wrapper. This project has no separate Application/CQRS layer (ADR-0001): handlers are
direct EF Core calls, so try/catch plus one exception-handling middleware is less
ceremony than threading `Result<T>` through every call site, and it maps directly onto
ASP.NET Core's built-in `ProblemDetails` support.
- `Domain` throws a small set of custom exceptions (e.g. `NotFoundException`,
  `ValidationException`) — never a bare `Exception`.
- `Api` registers one exception-handling middleware that maps these to `ProblemDetails`
  responses with the right HTTP status. Controllers don't catch exceptions themselves.
- Request DTOs are validated at the `Api` boundary before anything reaches
  `Domain`/`Infrastructure`.

## Patterns to follow
- `DbContext` used directly in `Api` (a small feature-shaped service only if a handler
  grows unwieldy) — no repository/unit-of-work abstraction (ADR-0001).
- Business rules that belong to an entity live on the entity, not in a controller.
- Controllers stay thin: parse request → call domain/EF Core → map to response DTO.
- A new entity/schema change ships its EF Core migration in the same change — schema and
  code move together.

## Patterns to avoid
- No generic repository/unit-of-work interface wrapping `DbContext` — decided against in
  ADR-0001; don't reintroduce it "for testability."
- No CQRS/MediatR pipeline — no separate Application layer at this scale (ADR-0001).
- Don't return `Domain` entities directly from `Api` endpoints, even when a DTO would look
  identical today — always map explicitly.
- Don't put EF Core-specific code (LINQ against `DbContext`, migrations) in `Domain` — it
  has no outward dependencies (ADR-0001).
- Don't catch exceptions inside individual controllers to build error responses — that
  bypasses the shared `ProblemDetails` middleware.
