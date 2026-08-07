using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FitLifePlanner.Api.Contracts.Progress;
using FitLifePlanner.Api.Contracts.Users;
using FitLifePlanner.Api.Contracts.Workouts;

namespace FitLifePlanner.Tests.Api.Progress;

public class WorkoutLogsControllerTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
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

    private static async Task<int> CreateExerciseAsync(HttpClient client, string name)
    {
        var exerciseResponse = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseRequest
        {
            Name = name,
            MuscleGroup = "Legs",
            Description = "Test exercise"
        });

        var exercise = await exerciseResponse.Content.ReadFromJsonAsync<ExerciseResponse>();
        return exercise!.Id;
    }

    [Fact]
    public async Task CreateWorkoutLog_and_add_entry_returns_created_with_matching_fields()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createLogRequest = new CreateWorkoutLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            Notes = "Good session",
            WorkoutPlanId = null
        };

        var logResponse = await client.PostAsJsonAsync("/api/workout-logs", createLogRequest);
        Assert.Equal(HttpStatusCode.Created, logResponse.StatusCode);
        var log = await logResponse.Content.ReadFromJsonAsync<WorkoutLogResponse>();

        Assert.Equal(createLogRequest.Notes, log!.Notes);
        Assert.Equal(createLogRequest.WorkoutPlanId, log.WorkoutPlanId);

        var exerciseId = await CreateExerciseAsync(client, "Squat");

        var addEntryRequest = new AddWorkoutLogEntryRequest
        {
            ExerciseId = exerciseId,
            SetsCompleted = 4,
            RepsCompleted = 8,
            WeightUsed = 80m
        };

        var addEntryResponse = await client.PostAsJsonAsync($"/api/workout-logs/{log.Id}/entries", addEntryRequest);

        Assert.Equal(HttpStatusCode.Created, addEntryResponse.StatusCode);
        var entry = await addEntryResponse.Content.ReadFromJsonAsync<WorkoutLogEntryResponse>();

        Assert.Equal(addEntryRequest.ExerciseId, entry!.ExerciseId);
        Assert.Equal(addEntryRequest.SetsCompleted, entry.SetsCompleted);
        Assert.Equal(addEntryRequest.RepsCompleted, entry.RepsCompleted);
        Assert.Equal(addEntryRequest.WeightUsed, entry.WeightUsed);
    }

    [Fact]
    public async Task CreateWorkoutLog_with_future_date_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createLogRequest = new CreateWorkoutLogRequest
        {
            Date = DateTime.UtcNow.AddDays(1),
            Notes = "Future session",
            WorkoutPlanId = null
        };

        var logResponse = await client.PostAsJsonAsync("/api/workout-logs", createLogRequest);

        Assert.Equal(HttpStatusCode.BadRequest, logResponse.StatusCode);
    }

    [Fact]
    public async Task AddEntry_with_non_positive_sets_completed_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var logResponse = await client.PostAsJsonAsync("/api/workout-logs", new CreateWorkoutLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            Notes = "Session",
            WorkoutPlanId = null
        });
        var log = await logResponse.Content.ReadFromJsonAsync<WorkoutLogResponse>();

        var exerciseId = await CreateExerciseAsync(client, "Deadlift");

        var addEntryRequest = new AddWorkoutLogEntryRequest
        {
            ExerciseId = exerciseId,
            SetsCompleted = 0,
            RepsCompleted = 8,
            WeightUsed = 80m
        };

        var addEntryResponse = await client.PostAsJsonAsync($"/api/workout-logs/{log!.Id}/entries", addEntryRequest);

        Assert.Equal(HttpStatusCode.BadRequest, addEntryResponse.StatusCode);
    }

    [Fact]
    public async Task AddEntry_with_non_positive_reps_completed_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var logResponse = await client.PostAsJsonAsync("/api/workout-logs", new CreateWorkoutLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            Notes = "Session",
            WorkoutPlanId = null
        });
        var log = await logResponse.Content.ReadFromJsonAsync<WorkoutLogResponse>();

        var exerciseId = await CreateExerciseAsync(client, "Overhead Press");

        var addEntryRequest = new AddWorkoutLogEntryRequest
        {
            ExerciseId = exerciseId,
            SetsCompleted = 3,
            RepsCompleted = 0,
            WeightUsed = 40m
        };

        var addEntryResponse = await client.PostAsJsonAsync($"/api/workout-logs/{log!.Id}/entries", addEntryRequest);

        Assert.Equal(HttpStatusCode.BadRequest, addEntryResponse.StatusCode);
    }

    [Fact]
    public async Task AddEntry_with_negative_weight_used_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var logResponse = await client.PostAsJsonAsync("/api/workout-logs", new CreateWorkoutLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            Notes = "Session",
            WorkoutPlanId = null
        });
        var log = await logResponse.Content.ReadFromJsonAsync<WorkoutLogResponse>();

        var exerciseId = await CreateExerciseAsync(client, "Lunge");

        var addEntryRequest = new AddWorkoutLogEntryRequest
        {
            ExerciseId = exerciseId,
            SetsCompleted = 3,
            RepsCompleted = 10,
            WeightUsed = -5m
        };

        var addEntryResponse = await client.PostAsJsonAsync($"/api/workout-logs/{log!.Id}/entries", addEntryRequest);

        Assert.Equal(HttpStatusCode.BadRequest, addEntryResponse.StatusCode);
    }
}
