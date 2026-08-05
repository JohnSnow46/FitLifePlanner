using FitLifePlanner.Domain.Nutrition;

namespace FitLifePlanner.Domain.Progress;

public class MealLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public MealType MealType { get; set; }
    public int FoodId { get; set; }
    public decimal QuantityConsumed { get; set; }
}
