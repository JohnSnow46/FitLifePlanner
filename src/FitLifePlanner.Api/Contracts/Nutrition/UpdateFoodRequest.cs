using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Nutrition;

public record UpdateFoodRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Unit { get; init; } = string.Empty;

    [Required]
    public decimal CaloriesPerUnit { get; init; }

    [Required]
    public decimal ProteinPerUnit { get; init; }

    [Required]
    public decimal CarbsPerUnit { get; init; }

    [Required]
    public decimal FatPerUnit { get; init; }
}
