namespace FitLifePlanner.Api.Contracts.Workouts;

public record WorkoutPlanExerciseResponse(
    int Id,
    int ExerciseId,
    int Order,
    int TargetSets,
    int TargetReps,
    decimal TargetWeight);
