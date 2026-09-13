using FitLifePlanner.Domain.Common;
using FitLifePlanner.Domain.Progress;

namespace FitLifePlanner.Tests.Domain.Progress;

public class BodyMetricEntryTests
{
    [Fact]
    public void Create_creates_body_metric_entry_with_given_data()
    {
        var date = DateTime.UtcNow.AddDays(-1);

        var entry = BodyMetricEntry.Create(userId: 1, date: date, weight: 80m, bodyFatPercent: 15m, notes: "Morning weigh-in");

        Assert.Equal(1, entry.UserId);
        Assert.Equal(date, entry.Date);
        Assert.Equal(80m, entry.Weight);
        Assert.Equal(15m, entry.BodyFatPercent);
        Assert.Equal("Morning weigh-in", entry.Notes);
    }

    [Fact]
    public void Create_with_future_date_throws_ValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);

        Assert.Throws<ValidationException>(() =>
            BodyMetricEntry.Create(userId: 1, date: futureDate, weight: 80m, bodyFatPercent: null, notes: ""));
    }

    [Fact]
    public void Create_with_non_positive_weight_throws_ValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            BodyMetricEntry.Create(userId: 1, date: DateTime.UtcNow, weight: 0m, bodyFatPercent: null, notes: ""));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.1)]
    public void Create_with_out_of_range_body_fat_percent_throws_ValidationException(decimal bodyFatPercent)
    {
        Assert.Throws<ValidationException>(() =>
            BodyMetricEntry.Create(userId: 1, date: DateTime.UtcNow, weight: 80m, bodyFatPercent: bodyFatPercent, notes: ""));
    }

    [Fact]
    public void Create_with_notes_exceeding_max_length_throws_ValidationException()
    {
        var notes = new string('a', 2001);

        Assert.Throws<ValidationException>(() =>
            BodyMetricEntry.Create(userId: 1, date: DateTime.UtcNow, weight: 80m, bodyFatPercent: null, notes: notes));
    }
}
