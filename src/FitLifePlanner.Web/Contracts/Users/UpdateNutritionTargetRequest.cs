using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Users;

public record UpdateNutritionTargetRequest
{
    [Range(0.01, double.MaxValue)]
    public decimal? DailyCalorieTarget { get; init; }
}
