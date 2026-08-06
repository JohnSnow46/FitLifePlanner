using FitLifePlanner.Domain.Common;
using FitLifePlanner.Domain.Nutrition;

namespace FitLifePlanner.Tests.Domain.Nutrition;

public class MealPlanTests
{
    [Fact]
    public void AddEntry_adds_entry_to_plan()
    {
        var plan = new MealPlan { Id = 1, UserId = 1, Name = "Cutting Plan" };

        var entry = plan.AddEntry(foodId: 10, mealType: MealType.Breakfast, quantity: 150m, dayOfWeek: DayOfWeek.Monday);

        var savedEntry = Assert.Single(plan.Entries);
        Assert.Same(entry, savedEntry);
        Assert.Equal(1, entry.MealPlanId);
        Assert.Equal(10, entry.FoodId);
        Assert.Equal(MealType.Breakfast, entry.MealType);
        Assert.Equal(150m, entry.Quantity);
        Assert.Equal(DayOfWeek.Monday, entry.DayOfWeek);
    }

    [Fact]
    public void AddEntry_with_duplicate_food_meal_type_and_day_throws_ValidationException()
    {
        var plan = new MealPlan { Id = 1, UserId = 1, Name = "Cutting Plan" };
        plan.AddEntry(foodId: 10, mealType: MealType.Breakfast, quantity: 150m, dayOfWeek: DayOfWeek.Monday);

        Assert.Throws<ValidationException>(() =>
            plan.AddEntry(foodId: 10, mealType: MealType.Breakfast, quantity: 200m, dayOfWeek: DayOfWeek.Monday));
    }

    [Fact]
    public void AddEntry_with_non_positive_quantity_throws_ValidationException()
    {
        var plan = new MealPlan { Id = 1, UserId = 1, Name = "Cutting Plan" };

        Assert.Throws<ValidationException>(() =>
            plan.AddEntry(foodId: 10, mealType: MealType.Breakfast, quantity: 0m, dayOfWeek: DayOfWeek.Monday));
    }
}
