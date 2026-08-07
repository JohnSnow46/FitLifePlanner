using System.ComponentModel.DataAnnotations;
using FitLifePlanner.Domain.Nutrition;

namespace FitLifePlanner.Api.Contracts.Progress;

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
