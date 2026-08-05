namespace FitLifePlanner.Domain.Progress;

public class WorkoutLogEntry
{
    public int Id { get; set; }
    public int WorkoutLogId { get; set; }
    public int ExerciseId { get; set; }
    public int SetsCompleted { get; set; }
    public int RepsCompleted { get; set; }
    public decimal WeightUsed { get; set; }
}
