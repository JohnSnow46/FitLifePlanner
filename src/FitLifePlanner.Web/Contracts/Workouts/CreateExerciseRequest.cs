using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Workouts;

public record CreateExerciseRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string MuscleGroup { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;
}
