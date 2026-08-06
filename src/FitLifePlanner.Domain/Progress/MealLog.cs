using FitLifePlanner.Domain.Common;
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

    public static MealLog Create(int userId, DateTime date, MealType mealType, int foodId, decimal quantityConsumed)
    {
        if (date > DateTime.UtcNow)
        {
            throw new ValidationException("Date cannot be in the future.");
        }

        if (quantityConsumed <= 0)
        {
            throw new ValidationException("Quantity consumed must be greater than zero.");
        }

        return new MealLog
        {
            UserId = userId,
            Date = date,
            MealType = mealType,
            FoodId = foodId,
            QuantityConsumed = quantityConsumed
        };
    }
}
