using FitLifePlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FitLifePlanner.Infrastructure.Postgres;

// Used only by `dotnet ef migrations add` (see docs/database.md §4). Takes precedence
// over Program.cs's own DbContext registration, so `dotnet ef` never runs through the
// API's host startup — which would otherwise call Database.Migrate() against this
// placeholder connection string before a migration even exists to generate.
public class PostgresDesignTimeDbContextFactory : IDesignTimeDbContextFactory<FitLifePlannerDbContext>
{
    public FitLifePlannerDbContext CreateDbContext(string[] args)
    {
        // No credentials here on purpose — `dotnet ef migrations add` only builds the
        // model, it never opens this connection, so there's nothing to authenticate.
        // Override via EF_DESIGN_TIME_CONNECTION_STRING if a real one is ever needed
        // (e.g. `dotnet ef database update` against a real Postgres instance).
        var connectionString = Environment.GetEnvironmentVariable("EF_DESIGN_TIME_CONNECTION_STRING")
            ?? "Host=localhost;Database=fitlifeplanner_design";

        var optionsBuilder = new DbContextOptionsBuilder<FitLifePlannerDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(PostgresDesignTimeDbContextFactory).Assembly.GetName().Name));

        return new FitLifePlannerDbContext(optionsBuilder.Options);
    }
}
