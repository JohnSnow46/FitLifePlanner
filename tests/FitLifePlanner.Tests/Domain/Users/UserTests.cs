using FitLifePlanner.Domain.Common;
using FitLifePlanner.Domain.Users;

namespace FitLifePlanner.Tests.Domain.Users;

public class UserTests
{
    [Fact]
    public void SetGoals_sets_target_weight_and_target_body_fat_percent()
    {
        var user = new User();

        user.SetGoals(targetWeight: 75m, targetBodyFatPercent: 15m);

        Assert.Equal(75m, user.TargetWeight);
        Assert.Equal(15m, user.TargetBodyFatPercent);
    }

    [Fact]
    public void SetGoals_with_null_values_clears_goals()
    {
        var user = new User();
        user.SetGoals(75m, 15m);

        user.SetGoals(null, null);

        Assert.Null(user.TargetWeight);
        Assert.Null(user.TargetBodyFatPercent);
    }

    [Fact]
    public void SetGoals_with_non_positive_target_weight_throws_ValidationException()
    {
        var user = new User();

        Assert.Throws<ValidationException>(() => user.SetGoals(0m, null));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.1)]
    public void SetGoals_with_out_of_range_target_body_fat_percent_throws_ValidationException(decimal targetBodyFatPercent)
    {
        var user = new User();

        Assert.Throws<ValidationException>(() => user.SetGoals(75m, targetBodyFatPercent));
    }
}
