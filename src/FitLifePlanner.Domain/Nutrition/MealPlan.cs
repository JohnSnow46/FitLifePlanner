using FitLifePlanner.Domain.Common;

namespace FitLifePlanner.Domain.Nutrition;

public class MealPlan
{
    private readonly List<MealPlanEntry> _entries = new();

    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    public IReadOnlyCollection<MealPlanEntry> Entries => _entries;

    public MealPlanEntry AddEntry(int foodId, MealType mealType, decimal quantity, DayOfWeek dayOfWeek)
    {
        if (_entries.Any(e => e.FoodId == foodId && e.MealType == mealType && e.DayOfWeek == dayOfWeek))
        {
            throw new ValidationException($"Food {foodId} is already planned for {mealType} on {dayOfWeek} in this meal plan.");
        }
        if (quantity <= 0)
        {
            throw new ValidationException("Quantity must be greater than zero.");
        }
        var entry = new MealPlanEntry
        {
            MealPlanId = Id,
            FoodId = foodId,
            MealType = mealType,
            Quantity = quantity,
            DayOfWeek = dayOfWeek
        };
        _entries.Add(entry);
        return entry;
    }
}
