using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Nutrition;

public record CreateFoodRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(50)]
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
