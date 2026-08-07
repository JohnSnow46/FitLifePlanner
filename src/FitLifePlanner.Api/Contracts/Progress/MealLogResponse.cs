using FitLifePlanner.Domain.Nutrition;

namespace FitLifePlanner.Api.Contracts.Progress;

public record MealLogResponse(int Id, DateTime Date, MealType MealType, int FoodId, decimal QuantityConsumed);
