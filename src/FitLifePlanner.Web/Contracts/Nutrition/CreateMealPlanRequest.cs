using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Nutrition;

public record CreateMealPlanRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;
}
