namespace FitLifePlanner.Web.Contracts.Progress;

public record WorkoutLogDetailResponse(
    int Id,
    DateTime Date,
    string Notes,
    int? WorkoutPlanId,
    IReadOnlyCollection<WorkoutLogEntryResponse> Entries);
