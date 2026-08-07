using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Workouts;

public record CreateWorkoutPlanRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;
}
