using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Nutrition;

public record UpdateMealPlanRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;
}
