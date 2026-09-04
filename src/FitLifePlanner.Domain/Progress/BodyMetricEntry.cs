using FitLifePlanner.Domain.Common;

namespace FitLifePlanner.Domain.Progress;

public class BodyMetricEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal Weight { get; set; }
    public decimal? BodyFatPercent { get; set; }
    public string Notes { get; set; } = string.Empty;

    public static BodyMetricEntry Create(int userId, DateTime date, decimal weight, decimal? bodyFatPercent, string notes)
    {
        if (date > DateTime.UtcNow)
        {
            throw new ValidationException("Date cannot be in the future.");
        }

        if (weight <= 0)
        {
            throw new ValidationException("Weight must be greater than zero.");
        }

        if (bodyFatPercent is < 0 or > 100)
        {
            throw new ValidationException("Body fat percent must be between 0 and 100.");
        }

        if (notes.Length > 2000)
        {
            throw new ValidationException("Notes must not exceed 2000 characters.");
        }

        return new BodyMetricEntry
        {
            UserId = userId,
            Date = date,
            Weight = weight,
            BodyFatPercent = bodyFatPercent,
            Notes = notes
        };
    }
}
