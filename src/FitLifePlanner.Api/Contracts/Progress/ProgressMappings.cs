using FitLifePlanner.Domain.Progress;

namespace FitLifePlanner.Api.Contracts.Progress;

public static class ProgressMappings
{
    public static WorkoutLogResponse ToResponse(this WorkoutLog log) =>
        new(log.Id, log.Date, log.Notes, log.WorkoutPlanId, log.TotalVolume());

    public static WorkoutLogDetailResponse ToDetailResponse(this WorkoutLog log) =>
        new(log.Id, log.Date, log.Notes, log.WorkoutPlanId, log.Entries.Select(e => e.ToResponse()).ToList());

    private static decimal TotalVolume(this WorkoutLog log) =>
        log.Entries.Sum(e => e.SetsCompleted * e.RepsCompleted * e.WeightUsed);

    public static WorkoutLogEntryResponse ToResponse(this WorkoutLogEntry entry) =>
        new(entry.Id, entry.ExerciseId, entry.SetsCompleted, entry.RepsCompleted, entry.WeightUsed);

    public static MealLogResponse ToResponse(this MealLog log) =>
        new(log.Id, log.Date, log.MealType, log.FoodId, log.QuantityConsumed);

    public static BodyMetricEntryResponse ToResponse(this BodyMetricEntry entry) =>
        new(entry.Id, entry.Date, entry.Weight, entry.BodyFatPercent, entry.Notes);
}
