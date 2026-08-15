using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Workouts;

public record UpdateExerciseRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string MuscleGroup { get; init; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;
}
