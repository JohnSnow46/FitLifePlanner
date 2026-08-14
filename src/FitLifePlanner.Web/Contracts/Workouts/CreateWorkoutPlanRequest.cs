using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Workouts;

public record CreateWorkoutPlanRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;
}
