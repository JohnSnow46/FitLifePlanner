using FitLifePlanner.Domain.Common;
using FitLifePlanner.Domain.Nutrition;
using FitLifePlanner.Domain.Progress;

namespace FitLifePlanner.Tests.Domain.Progress;

public class MealLogTests
{
    [Fact]
    public void Create_creates_meal_log_with_given_data()
    {
        var date = DateTime.UtcNow.AddDays(-1);

        var log = MealLog.Create(userId: 1, date: date, mealType: MealType.Lunch, foodId: 10, quantityConsumed: 200m);

        Assert.Equal(1, log.UserId);
        Assert.Equal(date, log.Date);
        Assert.Equal(MealType.Lunch, log.MealType);
        Assert.Equal(10, log.FoodId);
        Assert.Equal(200m, log.QuantityConsumed);
    }

    [Fact]
    public void Create_with_future_date_throws_ValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);

        Assert.Throws<ValidationException>(() =>
            MealLog.Create(userId: 1, date: futureDate, mealType: MealType.Lunch, foodId: 10, quantityConsumed: 200m));
    }

    [Fact]
    public void Create_with_non_positive_quantity_consumed_throws_ValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            MealLog.Create(userId: 1, date: DateTime.UtcNow, mealType: MealType.Lunch, foodId: 10, quantityConsumed: 0m));
    }
}
