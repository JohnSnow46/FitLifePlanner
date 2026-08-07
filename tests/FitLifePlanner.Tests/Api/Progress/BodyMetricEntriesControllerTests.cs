using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FitLifePlanner.Api.Contracts.Progress;
using FitLifePlanner.Api.Contracts.Users;

namespace FitLifePlanner.Tests.Api.Progress;

public class BodyMetricEntriesControllerTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
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
    public async Task CreateBodyMetricEntry_returns_created_with_matching_fields()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createRequest = new CreateBodyMetricEntryRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            Weight = 82.5m,
            BodyFatPercent = 18.2m,
            Notes = "Morning weigh-in"
        };

        var response = await client.PostAsJsonAsync("/api/body-metrics", createRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var entry = await response.Content.ReadFromJsonAsync<BodyMetricEntryResponse>();

        Assert.Equal(createRequest.Weight, entry!.Weight);
        Assert.Equal(createRequest.BodyFatPercent, entry.BodyFatPercent);
        Assert.Equal(createRequest.Notes, entry.Notes);
    }

    [Fact]
    public async Task CreateBodyMetricEntry_with_future_date_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createRequest = new CreateBodyMetricEntryRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            Weight = 80m,
            BodyFatPercent = null,
            Notes = string.Empty
        };

        var response = await client.PostAsJsonAsync("/api/body-metrics", createRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBodyMetricEntry_with_non_positive_weight_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createRequest = new CreateBodyMetricEntryRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            Weight = 0m,
            BodyFatPercent = null,
            Notes = string.Empty
        };

        var response = await client.PostAsJsonAsync("/api/body-metrics", createRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
