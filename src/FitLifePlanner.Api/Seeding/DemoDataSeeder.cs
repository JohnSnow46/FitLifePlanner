using FitLifePlanner.Domain.Nutrition;
using FitLifePlanner.Domain.Progress;
using FitLifePlanner.Domain.Users;
using FitLifePlanner.Domain.Workouts;
using FitLifePlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitLifePlanner.Api.Seeding;

/// <summary>
/// Populates a demo account + a few weeks of history so a fresh clone has something to
/// click through (ETAP 7 "demo polish" gap, docs/roadmap.md ETAP 9+). Triggered by
/// `dotnet run --project src/FitLifePlanner.Api -- --seed-demo`, which seeds and exits
/// instead of starting Kestrel. Idempotent: does nothing if the demo account already exists.
/// </summary>
public static class DemoDataSeeder
{
    public const string DemoEmail = "demo@fitlifeplanner.local";
    public const string DemoPassword = "Demo1234!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<FitLifePlannerDbContext>();
        var passwordHasher = services.GetRequiredService<PasswordHasher<User>>();

        if (await context.Users.AnyAsync(u => u.Email == DemoEmail))
        {
            Console.WriteLine($"Demo data already present (user {DemoEmail} exists) -- skipping seed.");
            return;
        }

        var user = new User { Name = "Demo Recruiter", Email = DemoEmail };
        user.PasswordHash = passwordHasher.HashPassword(user, DemoPassword);
        user.SetGoals(targetWeight: 78m, targetBodyFatPercent: 15m);
        user.SetNutritionTarget(dailyCalorieTarget: 2400m);
        context.Users.Add(user);

        var benchPress = new Exercise { Name = "Bench Press", MuscleGroup = "Chest", Description = "Barbell bench press.", IsFavorite = true };
        var squat = new Exercise { Name = "Squat", MuscleGroup = "Legs", Description = "Barbell back squat.", IsFavorite = true };
        var deadlift = new Exercise { Name = "Deadlift", MuscleGroup = "Back", Description = "Conventional deadlift." };
        var pullUp = new Exercise { Name = "Pull-up", MuscleGroup = "Back", Description = "Bodyweight pull-up." };
        var plank = new Exercise { Name = "Plank", MuscleGroup = "Core", Description = "Timed front plank hold." };
        context.Exercises.AddRange(benchPress, squat, deadlift, pullUp, plank);

        var chickenBreast = new Food { Name = "Chicken breast", Unit = "100g", CaloriesPerUnit = 165, ProteinPerUnit = 31, CarbsPerUnit = 0, FatPerUnit = 3.6m, IsFavorite = true };
        var rice = new Food { Name = "White rice (cooked)", Unit = "100g", CaloriesPerUnit = 130, ProteinPerUnit = 2.7m, CarbsPerUnit = 28, FatPerUnit = 0.3m };
        var banana = new Food { Name = "Banana", Unit = "1 medium", CaloriesPerUnit = 105, ProteinPerUnit = 1.3m, CarbsPerUnit = 27, FatPerUnit = 0.4m };
        var oats = new Food { Name = "Rolled oats", Unit = "100g", CaloriesPerUnit = 389, ProteinPerUnit = 17, CarbsPerUnit = 66, FatPerUnit = 7, IsFavorite = true };
        var oliveOil = new Food { Name = "Olive oil", Unit = "1 tbsp", CaloriesPerUnit = 119, ProteinPerUnit = 0, CarbsPerUnit = 0, FatPerUnit = 14 };
        context.Foods.AddRange(chickenBreast, rice, banana, oats, oliveOil);

        // First save: Exercise/Food/User rows need real, DB-generated ids before the
        // plans/logs below can reference them as foreign keys.
        await context.SaveChangesAsync();

        var workoutPlan = new WorkoutPlan { UserId = user.Id, Name = "Push/Pull/Legs", CreatedAt = DateTime.UtcNow };
        workoutPlan.AddExercise(benchPress.Id, order: 1, targetSets: 4, targetReps: 8, targetWeight: 60m);
        workoutPlan.AddExercise(squat.Id, order: 2, targetSets: 4, targetReps: 6, targetWeight: 80m);
        workoutPlan.AddExercise(deadlift.Id, order: 3, targetSets: 3, targetReps: 5, targetWeight: 100m);
        context.WorkoutPlans.Add(workoutPlan);

        var mealPlan = new MealPlan { UserId = user.Id, Name = "Cutting Plan" };
        mealPlan.AddEntry(oats.Id, MealType.Breakfast, quantity: 1, DayOfWeek.Monday);
        mealPlan.AddEntry(chickenBreast.Id, MealType.Lunch, quantity: 2, DayOfWeek.Monday);
        mealPlan.AddEntry(rice.Id, MealType.Lunch, quantity: 1.5m, DayOfWeek.Monday);
        mealPlan.AddEntry(banana.Id, MealType.Snack, quantity: 1, DayOfWeek.Monday);
        context.MealPlans.Add(mealPlan);

        // Second save: WorkoutLog.WorkoutPlanId is a plain FK column with no navigation
        // property for EF to fix up on its own, so workoutPlan needs a real, saved id
        // before the loop below can reference it.
        await context.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        for (var daysAgo = 14; daysAgo >= 0; daysAgo -= 2)
        {
            var logDate = today.AddDays(-daysAgo);

            var workoutLog = WorkoutLog.Create(user.Id, logDate, notes: "Felt strong today.", workoutPlanId: workoutPlan.Id);
            // Light progressive overload across the window, purely for a non-flat demo chart.
            var weightProgress = (15 - daysAgo) * 0.5m;
            workoutLog.AddEntry(benchPress.Id, setsCompleted: 4, repsCompleted: 8, weightUsed: 60m + weightProgress);
            workoutLog.AddEntry(squat.Id, setsCompleted: 4, repsCompleted: 6, weightUsed: 80m + weightProgress);
            context.WorkoutLogs.Add(workoutLog);

            context.MealLogs.Add(MealLog.Create(user.Id, logDate, MealType.Breakfast, oats.Id, quantityConsumed: 1));
            context.MealLogs.Add(MealLog.Create(user.Id, logDate, MealType.Lunch, chickenBreast.Id, quantityConsumed: 2));

            context.BodyMetricEntries.Add(BodyMetricEntry.Create(user.Id, logDate, weight: 82m - weightProgress * 0.1m, bodyFatPercent: 18m, notes: ""));
        }

        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded demo data. Log in with {DemoEmail} / {DemoPassword}");
    }
}
