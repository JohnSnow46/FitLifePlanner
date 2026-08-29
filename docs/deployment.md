# Deployment (Render)

Rationale for Render + PostgreSQL: `docs/adr/ADR-0005-hosting-and-production-database.md`.
This is a one-time manual setup in the Render dashboard — no IaC yet, not worth it at
this scale (`CLAUDE.md` priority order).

## 1. Database

1. Render dashboard → **New** → **PostgreSQL**. Free tier, any region close to the web
   services below.
2. Copy the **Internal Database URL** once it's provisioned — used as `Api`'s
   `ConnectionStrings__Default` in step 2. Render's URL is
   `postgres://user:pass@host/db`; EF Core/Npgsql needs the key=value form instead:
   `Host=<host>;Database=<db>;Username=<user>;Password=<pass>`.

## 2. API service

1. Render dashboard → **New** → **Web Service** → connect this repo, root directory `.`,
   **Dockerfile path** `src/FitLifePlanner.Api/Dockerfile`.
2. Environment variables:
   - `Database__Provider=Postgres`
   - `ConnectionStrings__Default=<Host=...;Database=...;Username=...;Password=...>` from
     step 1
   - `Jwt__Key=<a long random secret, generated once, never reused from local dev>`
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `Cors__AllowedOrigins__0=<Web service's Render URL, from step 3>` (Render assigns
     the URL before first deploy finishes — add this var, deploy Web first, then edit)
3. Deploy. `Program.cs` calls `Database.Migrate()` at startup — the schema is created
   automatically on first boot, no manual `dotnet ef database update` step against the
   Render database.
4. Verify: `<api-url>/health` returns healthy (checks the DB connection too — see
   `Program.cs`'s `AddDbContextCheck`).

## 3. Web service (Blazor WASM)

1. Render dashboard → **New** → **Web Service** → same repo, **Dockerfile path**
   `src/FitLifePlanner.Web/Dockerfile` (the same nginx-based image `docker-compose.yml`
   builds locally).
2. `Web/wwwroot/appsettings.Production.json`'s `ApiBaseUrl` currently points at
   `http://localhost:8080/` — the value `docker-compose.yml`'s host-mapped ports need
   locally. Before deploying to Render, change it to the API service's Render URL from
   step 2 (this file ships in the static build, so it needs a rebuild/redeploy of `Web`
   after editing, not an env var).
3. Deploy, then go back to the API service (step 2.2) and fill in
   `Cors__AllowedOrigins__0` with this service's URL, redeploy the API.

## Notes

- Render's free web services spin down after ~15 min of inactivity; the first request
  afterwards pays a 30-60s cold start. Worth a one-line callout in the README so a
  visitor doesn't read the delay as the app being broken.
- Two services deploy independently on every push to the connected branch — no shared
  build step with `docker-compose.yml`, which stays the local-only dev/test setup.
