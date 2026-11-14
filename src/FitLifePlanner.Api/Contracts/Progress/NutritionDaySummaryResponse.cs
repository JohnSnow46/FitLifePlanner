namespace FitLifePlanner.Api.Contracts.Progress;

public record NutritionDaySummaryResponse(DateTime Date, decimal Calories, decimal Protein, decimal Carbs, decimal Fat);
