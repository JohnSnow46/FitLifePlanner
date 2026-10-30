using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Users;

public record UpdateBodyGoalsRequest
{
    [Range(0.01, double.MaxValue)]
    public decimal? TargetWeight { get; init; }

    [Range(0, 100)]
    public decimal? TargetBodyFatPercent { get; init; }
}
