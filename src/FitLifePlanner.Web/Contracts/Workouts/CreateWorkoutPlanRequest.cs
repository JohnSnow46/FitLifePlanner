using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Workouts;

public record CreateWorkoutPlanRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;
}
