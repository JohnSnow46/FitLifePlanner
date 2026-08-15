using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Nutrition;

public record CreateMealPlanRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;
}
