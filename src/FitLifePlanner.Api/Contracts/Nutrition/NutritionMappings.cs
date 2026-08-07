using FitLifePlanner.Domain.Nutrition;

namespace FitLifePlanner.Api.Contracts.Nutrition;

public static class NutritionMappings
{
    public static FoodResponse ToResponse(this Food food) =>
        new(food.Id, food.Name, food.Unit, food.CaloriesPerUnit, food.ProteinPerUnit, food.CarbsPerUnit, food.FatPerUnit);

    public static MealPlanResponse ToResponse(this MealPlan plan) =>
        new(plan.Id, plan.Name);

    public static MealPlanDetailResponse ToDetailResponse(this MealPlan plan) =>
        new(plan.Id, plan.Name, plan.Entries.Select(e => e.ToResponse()).ToList());

    public static MealPlanEntryResponse ToResponse(this MealPlanEntry entry) =>
        new(entry.Id, entry.FoodId, entry.MealType, entry.Quantity, entry.DayOfWeek);
}
