using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Users;

public record UpdateNutritionTargetRequest
{
    [Range(0.01, double.MaxValue)]
    public decimal? DailyCalorieTarget { get; init; }
}
