namespace FitLifePlanner.Web.Contracts.Nutrition;

public record SetFoodFavoriteRequest
{
    public bool IsFavorite { get; init; }
}
