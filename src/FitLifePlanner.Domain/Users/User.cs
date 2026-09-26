using FitLifePlanner.Domain.Common;

namespace FitLifePlanner.Domain.Users;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public decimal? TargetWeight { get; set; }
    public decimal? TargetBodyFatPercent { get; set; }

    public void SetGoals(decimal? targetWeight, decimal? targetBodyFatPercent)
    {
        if (targetWeight <= 0)
        {
            throw new ValidationException("Target weight must be greater than zero.");
        }

        if (targetBodyFatPercent is < 0 or > 100)
        {
            throw new ValidationException("Target body fat percent must be between 0 and 100.");
        }

        TargetWeight = targetWeight;
        TargetBodyFatPercent = targetBodyFatPercent;
    }
}
