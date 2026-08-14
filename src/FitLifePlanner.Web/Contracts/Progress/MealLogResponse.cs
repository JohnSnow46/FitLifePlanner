using FitLifePlanner.Web.Contracts.Nutrition;

namespace FitLifePlanner.Web.Contracts.Progress;

public record MealLogResponse(int Id, DateTime Date, MealType MealType, int FoodId, decimal QuantityConsumed);
