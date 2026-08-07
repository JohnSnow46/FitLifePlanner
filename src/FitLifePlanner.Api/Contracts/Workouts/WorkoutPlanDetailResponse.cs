namespace FitLifePlanner.Api.Contracts.Workouts;

public record WorkoutPlanDetailResponse(
    int Id,
    string Name,
    DateTime CreatedAt,
    IReadOnlyCollection<WorkoutPlanExerciseResponse> Exercises);
