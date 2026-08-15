using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FitLifePlanner.Api.Contracts.Nutrition;
using FitLifePlanner.Api.Contracts.Progress;
using FitLifePlanner.Api.Contracts.Users;
using FitLifePlanner.Domain.Nutrition;

namespace FitLifePlanner.Tests.Api.Progress;

public class MealLogsControllerTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
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

    private static async Task<int> CreateFoodAsync(HttpClient client, string name)
    {
        var foodResponse = await client.PostAsJsonAsync("/api/foods", new CreateFoodRequest
        {
            Name = name,
            Unit = "g",
            CaloriesPerUnit = 1.5m,
            ProteinPerUnit = 0.2m,
            CarbsPerUnit = 0.1m,
            FatPerUnit = 0.05m
        });

        var food = await foodResponse.Content.ReadFromJsonAsync<FoodResponse>();
        return food!.Id;
    }

    [Fact]
    public async Task CreateMealLog_returns_created_with_matching_fields()
    {
        var client = await CreateAuthenticatedClientAsync();

        var foodId = await CreateFoodAsync(client, "Chicken Breast");

        var createLogRequest = new CreateMealLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            MealType = MealType.Lunch,
            FoodId = foodId,
            QuantityConsumed = 200m
        };

        var logResponse = await client.PostAsJsonAsync("/api/meal-logs", createLogRequest);

        Assert.Equal(HttpStatusCode.Created, logResponse.StatusCode);
        var log = await logResponse.Content.ReadFromJsonAsync<MealLogResponse>(JsonOptions);

        Assert.Equal(createLogRequest.MealType, log!.MealType);
        Assert.Equal(createLogRequest.FoodId, log.FoodId);
        Assert.Equal(createLogRequest.QuantityConsumed, log.QuantityConsumed);
    }

    [Fact]
    public async Task CreateMealLog_with_future_date_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var foodId = await CreateFoodAsync(client, "Rice");

        var createLogRequest = new CreateMealLogRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            MealType = MealType.Dinner,
            FoodId = foodId,
            QuantityConsumed = 150m
        };

        var logResponse = await client.PostAsJsonAsync("/api/meal-logs", createLogRequest);

        Assert.Equal(HttpStatusCode.BadRequest, logResponse.StatusCode);
    }

    [Fact]
    public async Task CreateMealLog_with_non_positive_quantity_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var foodId = await CreateFoodAsync(client, "Oats");

        var createLogRequest = new CreateMealLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            MealType = MealType.Breakfast,
            FoodId = foodId,
            QuantityConsumed = 0m
        };

        var logResponse = await client.PostAsJsonAsync("/api/meal-logs", createLogRequest);

        Assert.Equal(HttpStatusCode.BadRequest, logResponse.StatusCode);
    }

    [Fact]
    public async Task CreateMealLog_with_nonexistent_food_id_returns_not_found()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createLogRequest = new CreateMealLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            MealType = MealType.Breakfast,
            FoodId = 999_999,
            QuantityConsumed = 100m
        };

        var logResponse = await client.PostAsJsonAsync("/api/meal-logs", createLogRequest);

        Assert.Equal(HttpStatusCode.NotFound, logResponse.StatusCode);
    }

    [Fact]
    public async Task GetMealLog_owned_by_other_user_returns_not_found()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var foodId = await CreateFoodAsync(ownerClient, "Owner's Food");

        var logResponse = await ownerClient.PostAsJsonAsync("/api/meal-logs", new CreateMealLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            MealType = MealType.Lunch,
            FoodId = foodId,
            QuantityConsumed = 100m
        });
        var log = await logResponse.Content.ReadFromJsonAsync<MealLogResponse>(JsonOptions);

        var otherClient = await CreateAuthenticatedClientAsync();
        var response = await otherClient.GetAsync($"/api/meal-logs/{log!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
