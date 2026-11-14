using System.ComponentModel.DataAnnotations;
using FitLifePlanner.Api.Contracts.Common;

namespace FitLifePlanner.Api.Contracts.Progress;

public record CreateBodyMetricEntryRequest
{
    [Required]
    [NotInFuture]
    public DateTime Date { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Weight { get; init; }

    [Range(0, 100)]
    public decimal? BodyFatPercent { get; init; }

    [MaxLength(2000)]
    public string Notes { get; init; } = string.Empty;
}
