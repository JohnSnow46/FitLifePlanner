using FitLifePlanner.Domain.Nutrition;
using FitLifePlanner.Domain.Progress;
using FitLifePlanner.Domain.Users;
using FitLifePlanner.Domain.Workouts;
using Microsoft.EntityFrameworkCore;

namespace FitLifePlanner.Infrastructure.Data;

public class FitLifePlannerDbContext(DbContextOptions<FitLifePlannerDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<WorkoutPlanExercise> WorkoutPlanExercises => Set<WorkoutPlanExercise>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();
    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<WorkoutLogEntry> WorkoutLogEntries => Set<WorkoutLogEntry>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<BodyMetricEntry> BodyMetricEntries => Set<BodyMetricEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitLifePlannerDbContext).Assembly);
    }
}
