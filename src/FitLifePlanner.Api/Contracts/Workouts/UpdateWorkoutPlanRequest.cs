using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Workouts;

public record UpdateWorkoutPlanRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;
}
