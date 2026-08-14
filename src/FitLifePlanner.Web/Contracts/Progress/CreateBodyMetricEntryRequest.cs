using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Progress;

public record CreateBodyMetricEntryRequest
{
    [Required]
    public DateTime Date { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Weight { get; init; }

    public decimal? BodyFatPercent { get; init; }

    public string Notes { get; init; } = string.Empty;
}
