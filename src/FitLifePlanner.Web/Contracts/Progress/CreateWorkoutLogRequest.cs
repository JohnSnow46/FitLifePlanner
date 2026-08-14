using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Progress;

public record CreateWorkoutLogRequest
{
    [Required]
    public DateTime Date { get; init; }

    public string Notes { get; init; } = string.Empty;

    public int? WorkoutPlanId { get; init; }
}
