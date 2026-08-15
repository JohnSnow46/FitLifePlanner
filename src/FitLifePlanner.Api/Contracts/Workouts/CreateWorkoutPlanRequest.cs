using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Workouts;

public record CreateWorkoutPlanRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;
}
