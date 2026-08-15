using System.ComponentModel.DataAnnotations;
using FitLifePlanner.Web.Contracts.Nutrition;

namespace FitLifePlanner.Web.Contracts.Progress;

public record CreateMealLogRequest
{
    [Required]
    public DateTime Date { get; init; }

    [Required]
    public MealType MealType { get; init; }

    [Required]
    public int FoodId { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal QuantityConsumed { get; init; }
}
