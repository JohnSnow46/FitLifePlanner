namespace FitLifePlanner.Domain.Nutrition;

public class Food
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal CaloriesPerUnit { get; set; }
    public decimal ProteinPerUnit { get; set; }
    public decimal CarbsPerUnit { get; set; }
    public decimal FatPerUnit { get; set; }
}
