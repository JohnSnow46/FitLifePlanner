using FitLifePlanner.Domain.Common;

namespace FitLifePlanner.Domain.Workouts;

public class WorkoutPlan
{
    private readonly List<WorkoutPlanExercise> _exercises = new();

    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public IReadOnlyCollection<WorkoutPlanExercise> Exercises => _exercises;

    public WorkoutPlanExercise AddExercise(int exerciseId, int order, int targetSets, int targetReps, decimal targetWeight)
    {
        if (_exercises.Any(e => e.ExerciseId == exerciseId))
        {
            throw new ValidationException($"Exercise {exerciseId} is already part of this workout plan.");
        }

        if (_exercises.Any(e => e.Order == order))
        {
            throw new ValidationException($"Order {order} is already used by another exercise in this workout plan.");
        }

        var exercise = new WorkoutPlanExercise
        {
            WorkoutPlanId = Id,
            ExerciseId = exerciseId,
            Order = order,
            TargetSets = targetSets,
            TargetReps = targetReps,
            TargetWeight = targetWeight
        };

        _exercises.Add(exercise);
        return exercise;
    }
}
