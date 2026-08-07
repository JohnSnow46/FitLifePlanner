namespace FitLifePlanner.Api.Contracts.Progress;

public record WorkoutLogDetailResponse(
    int Id,
    DateTime Date,
    string Notes,
    int? WorkoutPlanId,
    IReadOnlyCollection<WorkoutLogEntryResponse> Entries);
