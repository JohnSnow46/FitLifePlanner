using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FitLifePlanner.Web.Contracts.Nutrition;

namespace FitLifePlanner.Web.Services;

public class NutritionApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<IReadOnlyCollection<FoodResponse>> GetFoodsAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("api/foods", cancellationToken);
        return await ReadResponseAsync<IReadOnlyCollection<FoodResponse>>(response, cancellationToken);
    }

    public async Task<FoodResponse> GetFoodAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/foods/{id}", cancellationToken);
        return await ReadResponseAsync<FoodResponse>(response, cancellationToken);
    }

    public async Task<FoodResponse> CreateFoodAsync(CreateFoodRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/foods", request, JsonOptions, cancellationToken);
        return await ReadResponseAsync<FoodResponse>(response, cancellationToken);
    }

    public async Task UpdateFoodAsync(int id, UpdateFoodRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/foods/{id}", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteFoodAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/foods/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MealPlanResponse>> GetMealPlansAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("api/meal-plans", cancellationToken);
        return await ReadResponseAsync<IReadOnlyCollection<MealPlanResponse>>(response, cancellationToken);
    }

    public async Task<MealPlanDetailResponse> GetMealPlanAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/meal-plans/{id}", cancellationToken);
        return await ReadResponseAsync<MealPlanDetailResponse>(response, cancellationToken);
    }

    public async Task<MealPlanResponse> CreateMealPlanAsync(CreateMealPlanRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/meal-plans", request, JsonOptions, cancellationToken);
        return await ReadResponseAsync<MealPlanResponse>(response, cancellationToken);
    }

    public async Task UpdateMealPlanAsync(int id, UpdateMealPlanRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/meal-plans/{id}", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteMealPlanAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/meal-plans/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<MealPlanEntryResponse> AddMealPlanEntryAsync(int planId, AddMealPlanEntryRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/meal-plans/{planId}/entries", request, JsonOptions, cancellationToken);
        return await ReadResponseAsync<MealPlanEntryResponse>(response, cancellationToken);
    }

    public async Task RemoveMealPlanEntryAsync(int planId, int entryId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/meal-plans/{planId}/entries/{entryId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var message = await ExtractErrorMessageAsync(response, cancellationToken);
            throw new ApiException(response.StatusCode, message);
        }

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return result!;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var message = await ExtractErrorMessageAsync(response, cancellationToken);
            throw new ApiException(response.StatusCode, message);
        }
    }

    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ProblemDetailsBody>(cancellationToken);
            if (!string.IsNullOrEmpty(body?.Detail))
            {
                return body.Detail;
            }
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or NotSupportedException)
        {
            // fall through to reason phrase below
        }

        return response.ReasonPhrase ?? "An unexpected error occurred.";
    }

    private record ProblemDetailsBody(string? Detail);
}
