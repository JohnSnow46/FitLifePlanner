using System.ComponentModel.DataAnnotations;
using FitLifePlanner.Api.Contracts.Common;

namespace FitLifePlanner.Api.Contracts.Progress;

public record CreateWorkoutLogRequest
{
    [Required]
    [NotInFuture]
    public DateTime Date { get; init; }

    [MaxLength(2000)]
    public string Notes { get; init; } = string.Empty;

    public int? WorkoutPlanId { get; init; }
}
