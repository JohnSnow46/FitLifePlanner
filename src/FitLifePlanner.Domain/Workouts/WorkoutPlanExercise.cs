namespace FitLifePlanner.Domain.Workouts;

public class WorkoutPlanExercise
{
    public int Id { get; set; }
    public int WorkoutPlanId { get; set; }
    public int ExerciseId { get; set; }
    public int Order { get; set; }
    public int TargetSets { get; set; }
    public int TargetReps { get; set; }
    public decimal TargetWeight { get; set; }
}
