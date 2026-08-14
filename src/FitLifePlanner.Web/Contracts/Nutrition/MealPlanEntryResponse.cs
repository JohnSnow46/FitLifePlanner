namespace FitLifePlanner.Web.Contracts.Nutrition;

public record MealPlanEntryResponse(
    int Id,
    int FoodId,
    MealType MealType,
    decimal Quantity,
    DayOfWeek DayOfWeek);
