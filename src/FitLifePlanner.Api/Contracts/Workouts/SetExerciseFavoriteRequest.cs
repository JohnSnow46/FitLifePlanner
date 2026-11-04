namespace FitLifePlanner.Api.Contracts.Workouts;

public record SetExerciseFavoriteRequest
{
    public bool IsFavorite { get; init; }
}
