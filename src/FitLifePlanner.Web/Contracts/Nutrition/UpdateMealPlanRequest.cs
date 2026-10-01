using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Nutrition;

public record UpdateMealPlanRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;
}
