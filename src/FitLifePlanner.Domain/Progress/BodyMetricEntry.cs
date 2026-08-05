namespace FitLifePlanner.Domain.Progress;

public class BodyMetricEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal Weight { get; set; }
    public decimal? BodyFatPercent { get; set; }
    public string Notes { get; set; } = string.Empty;
}
