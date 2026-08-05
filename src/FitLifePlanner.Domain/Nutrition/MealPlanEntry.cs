namespace FitLifePlanner.Domain.Nutrition;

public class MealPlanEntry
{
    public int Id { get; set; }
    public int MealPlanId { get; set; }
    public int FoodId { get; set; }
    public MealType MealType { get; set; }
    public decimal Quantity { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}
