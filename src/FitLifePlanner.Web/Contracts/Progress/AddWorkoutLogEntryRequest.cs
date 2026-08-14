using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Progress;

public record AddWorkoutLogEntryRequest
{
    [Required]
    public int ExerciseId { get; init; }

    [Range(1, int.MaxValue)]
    public int SetsCompleted { get; init; }

    [Range(1, int.MaxValue)]
    public int RepsCompleted { get; init; }

    [Range(0, double.MaxValue)]
    public decimal WeightUsed { get; init; }
}
