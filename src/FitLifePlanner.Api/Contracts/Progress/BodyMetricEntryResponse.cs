namespace FitLifePlanner.Api.Contracts.Progress;

public record BodyMetricEntryResponse(int Id, DateTime Date, decimal Weight, decimal? BodyFatPercent, string Notes);
