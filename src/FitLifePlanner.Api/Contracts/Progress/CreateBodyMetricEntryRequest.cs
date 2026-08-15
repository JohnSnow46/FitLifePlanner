using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Progress;

public record CreateBodyMetricEntryRequest
{
    [Required]
    public DateTime Date { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Weight { get; init; }

    public decimal? BodyFatPercent { get; init; }

    [MaxLength(2000)]
    public string Notes { get; init; } = string.Empty;
}
