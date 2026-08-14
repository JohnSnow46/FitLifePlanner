namespace FitLifePlanner.Web.Contracts.Progress;

public record WorkoutLogEntryResponse(
    int Id,
    int ExerciseId,
    int SetsCompleted,
    int RepsCompleted,
    decimal WeightUsed);
