namespace FitLifePlanner.Api.Contracts.Progress;

public record WorkoutLogEntryResponse(
    int Id,
    int ExerciseId,
    int SetsCompleted,
    int RepsCompleted,
    decimal WeightUsed);
