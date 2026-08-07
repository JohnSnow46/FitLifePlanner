namespace FitLifePlanner.Api.Contracts.Progress;

public record WorkoutLogResponse(int Id, DateTime Date, string Notes, int? WorkoutPlanId);
