using FitLifePlanner.Domain.Common;

namespace FitLifePlanner.Domain.Progress;

public class WorkoutLog
{
    private readonly List<WorkoutLogEntry> _entries = new();

    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int? WorkoutPlanId { get; set; }

    public IReadOnlyCollection<WorkoutLogEntry> Entries => _entries;

    public static WorkoutLog Create(int userId, DateTime date, string notes, int? workoutPlanId)
    {
        if (date > DateTime.UtcNow)
        {
            throw new ValidationException("Date cannot be in the future.");
        }

        if (notes.Length > 2000)
        {
            throw new ValidationException("Notes must not exceed 2000 characters.");
        }

        return new WorkoutLog
        {
            UserId = userId,
            Date = date,
            Notes = notes,
            WorkoutPlanId = workoutPlanId
        };
    }

    public WorkoutLogEntry AddEntry(int exerciseId, int setsCompleted, int repsCompleted, decimal weightUsed)
    {
        if (setsCompleted <= 0)
        {
            throw new ValidationException("Sets completed must be greater than zero.");
        }

        if (repsCompleted <= 0)
        {
            throw new ValidationException("Reps completed must be greater than zero.");
        }

        if (weightUsed < 0)
        {
            throw new ValidationException("Weight used cannot be negative.");
        }

        var entry = new WorkoutLogEntry
        {
            WorkoutLogId = Id,
            ExerciseId = exerciseId,
            SetsCompleted = setsCompleted,
            RepsCompleted = repsCompleted,
            WeightUsed = weightUsed
        };

        _entries.Add(entry);
        return entry;
    }
}
