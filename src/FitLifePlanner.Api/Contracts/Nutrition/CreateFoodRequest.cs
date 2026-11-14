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

    [Range(0, double.MaxValue)]
    public decimal CaloriesPerUnit { get; init; }

    [Range(0, double.MaxValue)]
    public decimal ProteinPerUnit { get; init; }

    [Range(0, double.MaxValue)]
    public decimal CarbsPerUnit { get; init; }

    [Range(0, double.MaxValue)]
    public decimal FatPerUnit { get; init; }
}
