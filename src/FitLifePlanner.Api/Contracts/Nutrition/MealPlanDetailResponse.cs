namespace FitLifePlanner.Api.Contracts.Nutrition;

public record MealPlanDetailResponse(
    int Id,
    string Name,
    IReadOnlyCollection<MealPlanEntryResponse> Entries);
