using FitLifePlanner.Domain.Common;
using FitLifePlanner.Domain.Progress;

namespace FitLifePlanner.Tests.Domain.Progress;

public class WorkoutLogTests
{
    [Fact]
    public void Create_creates_workout_log_with_given_data()
    {
        var date = DateTime.UtcNow.AddDays(-1);

        var log = WorkoutLog.Create(userId: 1, date: date, notes: "Felt strong", workoutPlanId: 5);

        Assert.Equal(1, log.UserId);
        Assert.Equal(date, log.Date);
        Assert.Equal("Felt strong", log.Notes);
        Assert.Equal(5, log.WorkoutPlanId);
        Assert.Empty(log.Entries);
    }

    [Fact]
    public void Create_with_future_date_throws_ValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);

        Assert.Throws<ValidationException>(() =>
            WorkoutLog.Create(userId: 1, date: futureDate, notes: "Future", workoutPlanId: null));
    }

    [Fact]
    public void Create_with_notes_over_2000_characters_throws_ValidationException()
    {
        var tooLongNotes = new string('a', 2001);

        Assert.Throws<ValidationException>(() =>
            WorkoutLog.Create(userId: 1, date: DateTime.UtcNow, notes: tooLongNotes, workoutPlanId: null));
    }

    [Fact]
    public void AddEntry_adds_entry_to_log()
    {
        var log = WorkoutLog.Create(userId: 1, date: DateTime.UtcNow, notes: "Leg day", workoutPlanId: null);
        log.Id = 1;

        var entry = log.AddEntry(exerciseId: 10, setsCompleted: 3, repsCompleted: 8, weightUsed: 40m);

        var savedEntry = Assert.Single(log.Entries);
        Assert.Same(entry, savedEntry);
        Assert.Equal(1, entry.WorkoutLogId);
        Assert.Equal(10, entry.ExerciseId);
        Assert.Equal(3, entry.SetsCompleted);
        Assert.Equal(8, entry.RepsCompleted);
        Assert.Equal(40m, entry.WeightUsed);
    }

    [Fact]
    public void AddEntry_with_non_positive_sets_completed_throws_ValidationException()
    {
        var log = WorkoutLog.Create(userId: 1, date: DateTime.UtcNow, notes: "Leg day", workoutPlanId: null);

        Assert.Throws<ValidationException>(() =>
            log.AddEntry(exerciseId: 10, setsCompleted: 0, repsCompleted: 8, weightUsed: 40m));
    }

    [Fact]
    public void AddEntry_with_non_positive_reps_completed_throws_ValidationException()
    {
        var log = WorkoutLog.Create(userId: 1, date: DateTime.UtcNow, notes: "Leg day", workoutPlanId: null);

        Assert.Throws<ValidationException>(() =>
            log.AddEntry(exerciseId: 10, setsCompleted: 3, repsCompleted: 0, weightUsed: 40m));
    }

    [Fact]
    public void AddEntry_with_negative_weight_used_throws_ValidationException()
    {
        var log = WorkoutLog.Create(userId: 1, date: DateTime.UtcNow, notes: "Leg day", workoutPlanId: null);

        Assert.Throws<ValidationException>(() =>
            log.AddEntry(exerciseId: 10, setsCompleted: 3, repsCompleted: 8, weightUsed: -1m));
    }
}
