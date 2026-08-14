# ADR-0004: Web (Blazor WASM) structure, auth state and API access

**Date:** 2026-08-13
**Status:** Accepted

**Context.**
ETAP 4 closed with a complete REST surface (`docs/api.md`) behind JWT bearer auth
(ADR-0003). `FitLifePlanner.Web` is still the untouched `blazorwasm` template — default
`Program.cs`, `Counter`/`Weather` sample pages, Bootstrap, and an `HttpClient` pointing at
the SPA's own origin. ADR-0001 fixed the stack (standalone Blazor WebAssembly, HTTP/JSON
only, **no project reference** to `Api`) and `project-conventions` already fixed the folder
layout (`Pages/<FeatureArea>/`, shared `Components/`, typed clients in `Services/`, never an
inline `HttpClient` call in a component). ADR-0003 §5 explicitly deferred one thing to this
decision: *"token storage on the SPA side is decided when that layer is built."*

The open questions this ADR answers, all of them frontend-wide and expensive to change
once a dozen pages exist: where the JWT lives, how it reaches the `Authorization` header,
how "am I logged in" propagates through the component tree, where the wire DTOs come from
given `Web` can't reference `Api`, whether any state-management library is introduced, and
what has to change on the `Api` side (CORS) for a cross-origin SPA to work at all.

## Decision

### 1. No state-management library, no UI kit

- **No Fluxor / Redux-style store, no `StateContainer` service.** Each page owns its own
  data: fetch in `OnInitializedAsync` through a typed client, hold it in page fields,
  re-fetch after a mutating call. Nothing in the MVP scope has cross-page shared mutable
  state — the one exception is authentication, which gets the framework's own mechanism
  (§4).
- **UI stays on the template's Bootstrap 5** plus hand-written Razor components; no
  MudBlazor/Radzen. Forms use built-in `EditForm` + `DataAnnotationsValidator` against
  form-model records with DataAnnotations, mirroring the request-DTO validation the `Api`
  already performs (the server stays the authority; client validation is UX only).

### 2. Wire models live in `Web/Contracts/<FeatureArea>/`

`Web` gets its **own** request/response records, hand-written to match the JSON produced by
`Api/Contracts/<FeatureArea>/`, in `src/FitLifePlanner.Web/Contracts/<FeatureArea>/`, using
the same `<Entity><Verb>Request` / `<Entity>Response` names as `Api`. No shared contracts
project, no project reference, no OpenAPI client generation at MVP. Only the DTOs a page
actually needs get written — this is not a mechanical mirror of all of `Api/Contracts`.

Deserialization uses `System.Net.Http.Json` defaults plus a `JsonStringEnumConverter`
(matching ADR-0003's global converter on the API side, so `MealType`/`DayOfWeek` round-trip
as strings); all `DateTime` values are UTC on the wire.

### 3. Typed API clients over `IHttpClientFactory` + a bearer `DelegatingHandler`

- One typed client per API feature area, named `<FeatureArea>ApiClient`
  (`UsersApiClient`, `WorkoutsApiClient`, `NutritionApiClient`, `ProgressApiClient`) in
  `src/FitLifePlanner.Web/Services/`, registered with
  `builder.Services.AddHttpClient<TClient>(c => c.BaseAddress = apiBaseUrl)`. Components
  inject the client, never `HttpClient`.
- **The base address comes from configuration**, not from `HostEnvironment.BaseAddress`:
  `ApiBaseUrl` in `wwwroot/appsettings.json` (and `appsettings.Development.json`), read via
  `builder.Configuration["ApiBaseUrl"]`. Local dev value is the API's HTTPS profile
  (`https://localhost:7152/`). Hosting is still undecided (ADR-0001), so the SPA must not
  assume same-origin.
- **The `Authorization` header is attached by one `BearerTokenHandler : DelegatingHandler`**
  registered on every typed client via `.AddHttpMessageHandler<BearerTokenHandler>()` —
  never per call site. It reads the token from `TokenStore` (§4) and adds
  `Authorization: Bearer <token>` when one exists.
- **The same handler owns the 401 reaction:** on a `401` response it clears the stored
  token and notifies the auth state provider, so an expired 7-day token degrades into
  "logged out" instead of a page full of failed requests. Redirecting to `/login` is the
  router's job (§4), not the handler's.
- **Failures surface as an exception, not a return code.** Non-success responses are turned
  into `ApiException(HttpStatusCode statusCode, string message)`
  (`src/FitLifePlanner.Web/Services/ApiException.cs`), with `message` taken from the
  `ProblemDetails.detail` the API's `GlobalExceptionHandler` returns (falling back to the
  reason phrase). This mirrors the "exceptions, not `Result<T>`" rule from
  `project-conventions`. Pages catch `ApiException` around mutating calls and render the
  message through a shared error component; they don't catch to inspect status codes,
  except `401`, which never reaches them.

### 4. JWT in `localStorage`, auth state via `AuthenticationStateProvider`

- **Storage: browser `localStorage`, key `fitlife.token`**, accessed through a
  `TokenStore` service (`Services/TokenStore.cs`) that wraps `IJSRuntime` calls to
  `localStorage.getItem/setItem/removeItem` and caches the value in a field. No
  `Blazored.LocalStorage` package — three interop calls don't justify a dependency.
  `localStorage` (not `sessionStorage`) so a refresh or a second tab keeps the 7-day token.
- **Auth state: the framework's own mechanism.** Add the
  `Microsoft.AspNetCore.Components.Authorization` package and a custom
  `JwtAuthenticationStateProvider : AuthenticationStateProvider` that reads the token from
  `TokenStore`, parses its payload into `Claim`s (`sub` → `ClaimTypes.NameIdentifier`,
  `email`), treats a missing/malformed token **or a passed `exp`** as anonymous (dropping
  the token), and exposes `SignIn(token)` / `SignOut()` which persist/clear the token and
  call `NotifyAuthenticationStateChanged`. The token is *not* validated cryptographically
  client-side — that is the API's job; the client only reads it to decide what to render.
- **`App.razor` uses `<CascadingAuthenticationState>` + `<AuthorizeRouteView>`**, with a
  `NotAuthorized` fragment that redirects to `/login` (preserving the attempted URL as a
  `returnUrl` query parameter). Pages are protected by `[Authorize]`; `/login` and
  `/register` carry `[AllowAnonymous]` — i.e. the SPA mirrors the API's
  authenticated-by-default posture from ADR-0003. `NavMenu` shows the feature links and the
  logout action inside `<AuthorizeView>`.
- **Known trade-off:** a token in `localStorage` is readable by any injected script. That is
  the accepted cost of the bearer-token model ADR-0003 already chose over cookies for an
  origin-agnostic standalone SPA; there is no HttpOnly-cookie option that doesn't reopen
  that decision.

### 5. CORS on the `Api`

`Api/Program.cs` gains a named CORS policy allowing the origins listed in
`Cors:AllowedOrigins` (configuration; local dev = the Web project's `https://localhost:7122`
and `http://localhost:5221`) with any header and method, `UseCors` placed before
`UseAuthentication`. **No `AllowCredentials`** — the token travels in a header, not a
cookie. This is the only `Api` change ETAP 5 requires.

## Alternatives considered

- **A shared `FitLifePlanner.Contracts` project referenced by `Api` and `Web`.** Rejected
  for now — it means a fifth project and moving the whole existing `Api/Contracts` tree,
  i.e. refactoring working ETAP 4 code to save typing in ETAP 5. The cost of the chosen
  option is manual drift; the mitigation is that both sides are in one repo and one commit,
  and extracting the shared project later is a mechanical move, not a redesign.
- **Generating a client from the OpenAPI document** (`AddOpenApi` is already registered).
  Rejected at MVP — a generator (NSwag/Kiota) plus a build step and generated-code review
  is more machinery than ~15 hand-written records, and it fights the "only the DTOs a page
  needs" rule.
- **`sessionStorage` or in-memory-only token.** Rejected — losing the session on every F5
  makes the app feel broken during development, and the XSS exposure difference is marginal
  for a single-user portfolio app.
- **`Microsoft.AspNetCore.Components.WebAssembly.Authentication`.** Rejected — it is built
  around OIDC/`IAccessTokenProvider` flows against an identity provider; ADR-0003 chose a
  hand-rolled JWT endpoint pair, so only the lightweight
  `Components.Authorization` half is relevant.
- **A custom cascading `AuthState` service instead of `AuthenticationStateProvider`.**
  Rejected — it would reimplement `<AuthorizeView>`/`[Authorize]`/`AuthorizeRouteView`,
  which are already in the box and which every Blazor reader recognises.
- **Setting the `Authorization` header inside each typed client method.** Rejected — one
  forgotten call site is a silent 401, and it duplicates the 401-handling logic four times.
- **Fluxor / a global store.** Rejected — no MVP screen depends on another screen's mutable
  state; a store would be ceremony around `OnInitializedAsync` + re-fetch.
- **Hosting the WASM app from the `Api` project (same origin, no CORS).** Rejected — it
  contradicts ADR-0001's standalone-SPA layering and would fold the hosting decision
  (deliberately still open) into a frontend task.

## Consequences

- New `Web` packages: `Microsoft.AspNetCore.Components.Authorization` only. `Api` gains no
  package (CORS is built in); `Domain`/`Infrastructure` are untouched, and there is **no
  schema or migration change** in this stage.
- Running the app locally now requires **both** projects running (`dotnet run` on `Api` and
  on `Web`); `docs/development.md`'s local-dev section stays valid but the SPA is useless
  without the API up.
- Every later frontend stage is mechanical: add `Contracts/<FeatureArea>` records → add
  methods to the `<FeatureArea>ApiClient` → add pages under `Pages/<FeatureArea>/`. No page
  touches `HttpClient`, tokens, or headers.
- Testing: no bUnit at this stage. The pieces worth testing here are pure — JWT payload
  → claims parsing and expiry handling in `JwtAuthenticationStateProvider` — and are
  covered by plain xUnit tests in `tests/FitLifePlanner.Tests/Web/`. Component rendering is
  validated manually (register → refresh → still logged in → logout). Introducing bUnit is a
  later, separate decision if page logic grows.
- ADR-0003's "no `PUT` on plan children and logs" means the UI's edit affordance for those
  is delete + re-add; the pages are designed around that rather than working around it.
- When hosting is decided, the only frontend-visible changes are `ApiBaseUrl` in
  `wwwroot/appsettings.json` and the `Cors:AllowedOrigins` list — both configuration, not
  code.
