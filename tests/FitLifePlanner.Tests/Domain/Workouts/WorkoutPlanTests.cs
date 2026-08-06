using FitLifePlanner.Domain.Common;
using FitLifePlanner.Domain.Workouts;

namespace FitLifePlanner.Tests.Domain.Workouts;

public class WorkoutPlanTests
{
    [Fact]
    public void AddExercise_adds_exercise_to_plan()
    {
        var plan = new WorkoutPlan { Id = 1, UserId = 1, Name = "Push Day", CreatedAt = DateTime.UtcNow };

        var exercise = plan.AddExercise(exerciseId: 10, order: 1, targetSets: 3, targetReps: 10, targetWeight: 50m);

        var savedExercise = Assert.Single(plan.Exercises);
        Assert.Same(exercise, savedExercise);
        Assert.Equal(1, exercise.WorkoutPlanId);
        Assert.Equal(10, exercise.ExerciseId);
        Assert.Equal(1, exercise.Order);
        Assert.Equal(3, exercise.TargetSets);
        Assert.Equal(10, exercise.TargetReps);
        Assert.Equal(50m, exercise.TargetWeight);
    }

    [Fact]
    public void AddExercise_with_duplicate_exercise_id_throws_ValidationException()
    {
        var plan = new WorkoutPlan { Id = 1, UserId = 1, Name = "Push Day", CreatedAt = DateTime.UtcNow };
        plan.AddExercise(exerciseId: 10, order: 1, targetSets: 3, targetReps: 10, targetWeight: 50m);

        Assert.Throws<ValidationException>(() =>
            plan.AddExercise(exerciseId: 10, order: 2, targetSets: 4, targetReps: 8, targetWeight: 60m));
    }

    [Fact]
    public void AddExercise_with_duplicate_order_throws_ValidationException()
    {
        var plan = new WorkoutPlan { Id = 1, UserId = 1, Name = "Push Day", CreatedAt = DateTime.UtcNow };
        plan.AddExercise(exerciseId: 10, order: 1, targetSets: 3, targetReps: 10, targetWeight: 50m);

        Assert.Throws<ValidationException>(() =>
            plan.AddExercise(exerciseId: 20, order: 1, targetSets: 4, targetReps: 8, targetWeight: 60m));
    }
}
