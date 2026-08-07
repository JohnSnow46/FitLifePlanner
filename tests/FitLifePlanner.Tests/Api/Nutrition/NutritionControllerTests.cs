using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FitLifePlanner.Api.Contracts.Nutrition;
using FitLifePlanner.Api.Contracts.Users;
using FitLifePlanner.Domain.Nutrition;

namespace FitLifePlanner.Tests.Api.Nutrition;

public class NutritionControllerTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequest
        {
            Name = "Jan Kowalski",
            Email = $"{Guid.NewGuid()}@example.com",
            Password = "correct-horse-battery"
        });

        var body = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);

        return client;
    }

    [Fact]
    public async Task AddEntry_to_meal_plan_returns_created_with_matching_fields()
    {
        var client = await CreateAuthenticatedClientAsync();

        var planResponse = await client.PostAsJsonAsync("/api/meal-plans", new CreateMealPlanRequest
        {
            Name = "Week 1"
        });
        Assert.Equal(HttpStatusCode.Created, planResponse.StatusCode);
        var plan = await planResponse.Content.ReadFromJsonAsync<MealPlanResponse>();

        var foodResponse = await client.PostAsJsonAsync("/api/foods", new CreateFoodRequest
        {
            Name = "Chicken Breast",
            Unit = "g",
            CaloriesPerUnit = 1.65m,
            ProteinPerUnit = 0.31m,
            CarbsPerUnit = 0m,
            FatPerUnit = 0.036m
        });
        Assert.Equal(HttpStatusCode.Created, foodResponse.StatusCode);
        var food = await foodResponse.Content.ReadFromJsonAsync<FoodResponse>();

        var addEntryRequest = new AddMealPlanEntryRequest
        {
            FoodId = food!.Id,
            MealType = MealType.Lunch,
            Quantity = 200m,
            DayOfWeek = DayOfWeek.Monday
        };

        var addEntryResponse = await client.PostAsJsonAsync($"/api/meal-plans/{plan!.Id}/entries", addEntryRequest);

        Assert.Equal(HttpStatusCode.Created, addEntryResponse.StatusCode);
        var entry = await addEntryResponse.Content.ReadFromJsonAsync<MealPlanEntryResponse>(JsonOptions);

        Assert.Equal(addEntryRequest.FoodId, entry!.FoodId);
        Assert.Equal(addEntryRequest.MealType, entry.MealType);
        Assert.Equal(addEntryRequest.Quantity, entry.Quantity);
        Assert.Equal(addEntryRequest.DayOfWeek, entry.DayOfWeek);
    }

    [Fact]
    public async Task AddEntry_with_duplicate_combination_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var planResponse = await client.PostAsJsonAsync("/api/meal-plans", new CreateMealPlanRequest
        {
            Name = "Week 2"
        });
        var plan = await planResponse.Content.ReadFromJsonAsync<MealPlanResponse>();

        var foodResponse = await client.PostAsJsonAsync("/api/foods", new CreateFoodRequest
        {
            Name = "Rice",
            Unit = "g",
            CaloriesPerUnit = 1.3m,
            ProteinPerUnit = 0.027m,
            CarbsPerUnit = 0.28m,
            FatPerUnit = 0.003m
        });
        var food = await foodResponse.Content.ReadFromJsonAsync<FoodResponse>();

        var addEntryRequest = new AddMealPlanEntryRequest
        {
            FoodId = food!.Id,
            MealType = MealType.Dinner,
            Quantity = 150m,
            DayOfWeek = DayOfWeek.Tuesday
        };

        var firstAddResponse = await client.PostAsJsonAsync($"/api/meal-plans/{plan!.Id}/entries", addEntryRequest);
        Assert.Equal(HttpStatusCode.Created, firstAddResponse.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync($"/api/meal-plans/{plan.Id}/entries", addEntryRequest);

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task AddEntry_with_non_positive_quantity_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var planResponse = await client.PostAsJsonAsync("/api/meal-plans", new CreateMealPlanRequest
        {
            Name = "Week 3"
        });
        var plan = await planResponse.Content.ReadFromJsonAsync<MealPlanResponse>();

        var foodResponse = await client.PostAsJsonAsync("/api/foods", new CreateFoodRequest
        {
            Name = "Oats",
            Unit = "g",
            CaloriesPerUnit = 3.89m,
            ProteinPerUnit = 0.169m,
            CarbsPerUnit = 0.663m,
            FatPerUnit = 0.069m
        });
        var food = await foodResponse.Content.ReadFromJsonAsync<FoodResponse>();

        var addEntryRequest = new AddMealPlanEntryRequest
        {
            FoodId = food!.Id,
            MealType = MealType.Breakfast,
            Quantity = 0m,
            DayOfWeek = DayOfWeek.Wednesday
        };

        var addEntryResponse = await client.PostAsJsonAsync($"/api/meal-plans/{plan!.Id}/entries", addEntryRequest);

        Assert.Equal(HttpStatusCode.BadRequest, addEntryResponse.StatusCode);
    }
}
