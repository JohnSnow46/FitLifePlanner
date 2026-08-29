using System.Text;
using System.Text.Json.Serialization;
using FitLifePlanner.Api.Middleware;
using FitLifePlanner.Api.Services;
using FitLifePlanner.Domain.Users;
using FitLifePlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// "Database:Provider" selects the production engine (see ADR-0005); defaults to
// Sqlite so local dev/tests need no configuration. Postgres migrations live in a
// separate assembly (FitLifePlanner.Infrastructure.Postgres) — see docs/database.md §4
// for why a single migrations history can't serve two providers.
var databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
builder.Services.AddDbContext<FitLifePlannerDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");
    if (string.Equals(databaseProvider, "Postgres", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsAssembly("FitLifePlanner.Infrastructure.Postgres"));
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<FitLifePlannerDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<PasswordHasher<User>>();
builder.Services.AddScoped<TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Read lazily (not into a local before Build()) so WebApplicationFactory-based
        // tests can override Jwt:* after the builder is constructed but before this
        // delegate is first invoked by the options system.
        var jwtKey = builder.Configuration["Jwt:Key"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply pending EF Core migrations at startup so a fresh db (e.g. an empty Docker
// volume) gets its schema without a manual `dotnet ef database update` step.
// Skipped in the "Testing" environment, where TestApiFactory builds its own
// in-memory SQLite schema via EnsureCreated().
if (!app.Environment.IsEnvironment("Testing"))
{
    using var migrationScope = app.Services.CreateScope();
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<FitLifePlannerDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors("WebClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
