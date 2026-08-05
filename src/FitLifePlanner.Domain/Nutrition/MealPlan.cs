namespace FitLifePlanner.Domain.Nutrition;

public class MealPlan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}
