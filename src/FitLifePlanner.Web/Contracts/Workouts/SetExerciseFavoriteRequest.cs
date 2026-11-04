namespace FitLifePlanner.Web.Contracts.Workouts;

public record SetExerciseFavoriteRequest
{
    public bool IsFavorite { get; init; }
}
