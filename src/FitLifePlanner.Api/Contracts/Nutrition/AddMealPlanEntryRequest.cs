using System.ComponentModel.DataAnnotations;
using FitLifePlanner.Domain.Nutrition;

namespace FitLifePlanner.Api.Contracts.Nutrition;

public record AddMealPlanEntryRequest
{
    [Required]
    public int FoodId { get; init; }

    [Required]
    [EnumDataType(typeof(MealType))]
    public MealType MealType { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; init; }

    [Required]
    public DayOfWeek DayOfWeek { get; init; }
}
