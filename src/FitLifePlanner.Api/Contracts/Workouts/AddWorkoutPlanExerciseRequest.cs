using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Workouts;

public record AddWorkoutPlanExerciseRequest
{
    [Required]
    public int ExerciseId { get; init; }

    [Range(0, int.MaxValue)]
    public int Order { get; init; }

    [Range(0, int.MaxValue)]
    public int TargetSets { get; init; }

    [Range(0, int.MaxValue)]
    public int TargetReps { get; init; }

    [Range(0, double.MaxValue)]
    public decimal TargetWeight { get; init; }
}
