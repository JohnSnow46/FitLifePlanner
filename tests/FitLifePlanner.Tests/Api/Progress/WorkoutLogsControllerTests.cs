using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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

    [Fact]
    public async Task AddEntry_with_nonexistent_exercise_id_returns_not_found()
    {
        var client = await CreateAuthenticatedClientAsync();

        var logResponse = await client.PostAsJsonAsync("/api/workout-logs", new CreateWorkoutLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            Notes = "Session",
            WorkoutPlanId = null
        });
        var log = await logResponse.Content.ReadFromJsonAsync<WorkoutLogResponse>();

        var addEntryRequest = new AddWorkoutLogEntryRequest
        {
            ExerciseId = 999_999,
            SetsCompleted = 3,
            RepsCompleted = 10,
            WeightUsed = 40m
        };

        var addEntryResponse = await client.PostAsJsonAsync($"/api/workout-logs/{log!.Id}/entries", addEntryRequest);

        Assert.Equal(HttpStatusCode.NotFound, addEntryResponse.StatusCode);
    }

    [Fact]
    public async Task CreateWorkoutLog_with_other_users_workout_plan_id_returns_not_found()
    {
        var otherUserClient = await CreateAuthenticatedClientAsync();
        var otherPlanResponse = await otherUserClient.PostAsJsonAsync("/api/workout-plans", new CreateWorkoutPlanRequest
        {
            Name = "Someone Else's Plan"
        });
        var otherPlan = await otherPlanResponse.Content.ReadFromJsonAsync<WorkoutPlanResponse>();

        var client = await CreateAuthenticatedClientAsync();

        var createLogRequest = new CreateWorkoutLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            Notes = "Borrowed plan attempt",
            WorkoutPlanId = otherPlan!.Id
        };

        var logResponse = await client.PostAsJsonAsync("/api/workout-logs", createLogRequest);

        Assert.Equal(HttpStatusCode.NotFound, logResponse.StatusCode);
    }

    [Fact]
    public async Task GetWorkoutLog_owned_by_other_user_returns_not_found()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var logResponse = await ownerClient.PostAsJsonAsync("/api/workout-logs", new CreateWorkoutLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            Notes = "Owner's session",
            WorkoutPlanId = null
        });
        var log = await logResponse.Content.ReadFromJsonAsync<WorkoutLogResponse>();

        var otherClient = await CreateAuthenticatedClientAsync();
        var response = await otherClient.GetAsync($"/api/workout-logs/{log!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkoutLog_with_offset_date_returns_consistent_utc_instant_on_get()
    {
        var client = await CreateAuthenticatedClientAsync();

        const string offsetDateText = "2026-08-14T22:30:00+02:00";
        var expectedUtc = DateTimeOffset.Parse(offsetDateText, CultureInfo.InvariantCulture).UtcDateTime;

        var json = $$"""{"date":"{{offsetDateText}}","notes":"tz test","workoutPlanId":null}""";
        var postResponse = await client.PostAsync("/api/workout-logs", new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var posted = await postResponse.Content.ReadFromJsonAsync<WorkoutLogResponse>();

        var listResponse = await client.GetAsync("/api/workout-logs");
        var logs = await listResponse.Content.ReadFromJsonAsync<List<WorkoutLogResponse>>();
        var fetched = logs!.Single(l => l.Id == posted!.Id);

        // Both the POST echo and the subsequent GET must resolve to the same absolute
        // instant as the offset the client actually sent, not the same wall-clock digits.
        Assert.Equal(expectedUtc, posted!.Date.ToUniversalTime());
        Assert.Equal(expectedUtc, fetched.Date.ToUniversalTime());
        Assert.Equal(DateTimeKind.Utc, fetched.Date.Kind);
    }

    [Fact]
    public async Task CreateWorkoutLog_with_no_offset_date_is_treated_as_already_utc()
    {
        var client = await CreateAuthenticatedClientAsync();

        const string plainDateText = "2026-08-14T22:30:00";
        var expectedUtc = DateTime.SpecifyKind(DateTime.Parse(plainDateText, CultureInfo.InvariantCulture), DateTimeKind.Utc);

        var json = $$"""{"date":"{{plainDateText}}","notes":"tz test","workoutPlanId":null}""";
        var postResponse = await client.PostAsync("/api/workout-logs", new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var posted = await postResponse.Content.ReadFromJsonAsync<WorkoutLogResponse>();

        var listResponse = await client.GetAsync("/api/workout-logs");
        var logs = await listResponse.Content.ReadFromJsonAsync<List<WorkoutLogResponse>>();
        var fetched = logs!.Single(l => l.Id == posted!.Id);

        Assert.Equal(expectedUtc, fetched.Date);
    }
}
