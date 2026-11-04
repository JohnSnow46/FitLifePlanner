namespace FitLifePlanner.Api.Contracts.Nutrition;

public record SetFoodFavoriteRequest
{
    public bool IsFavorite { get; init; }
}
