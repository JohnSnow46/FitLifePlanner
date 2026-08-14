using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Nutrition;

public record AddMealPlanEntryRequest
{
    [Required]
    public int FoodId { get; init; }

    [Required]
    public MealType MealType { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; init; }

    [Required]
    public DayOfWeek DayOfWeek { get; init; }
}
