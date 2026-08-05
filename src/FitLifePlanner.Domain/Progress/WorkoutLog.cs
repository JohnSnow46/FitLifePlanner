namespace FitLifePlanner.Domain.Progress;

public class WorkoutLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int? WorkoutPlanId { get; set; }
}
