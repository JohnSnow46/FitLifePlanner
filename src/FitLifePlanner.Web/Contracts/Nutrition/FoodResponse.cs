namespace FitLifePlanner.Web.Contracts.Nutrition;

public record FoodResponse(
    int Id,
    string Name,
    string Unit,
    decimal CaloriesPerUnit,
    decimal ProteinPerUnit,
    decimal CarbsPerUnit,
    decimal FatPerUnit);
