using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FitLifePlanner.Api.Contracts.Progress;
using FitLifePlanner.Api.Contracts.Users;
using FitLifePlanner.Api.Contracts.Workouts;

namespace FitLifePlanner.Tests.Api.Workouts;

public class WorkoutsControllerTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
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
    public async Task AddExercise_to_workout_plan_returns_created_with_matching_fields()
    {
        var client = await CreateAuthenticatedClientAsync();

        var planResponse = await client.PostAsJsonAsync("/api/workout-plans", new CreateWorkoutPlanRequest
        {
            Name = "Push Day"
        });
        Assert.Equal(HttpStatusCode.Created, planResponse.StatusCode);
        var plan = await planResponse.Content.ReadFromJsonAsync<WorkoutPlanResponse>();

        var exerciseResponse = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseRequest
        {
            Name = "Bench Press",
            MuscleGroup = "Chest",
            Description = "Barbell bench press"
        });
        Assert.Equal(HttpStatusCode.Created, exerciseResponse.StatusCode);
        var exercise = await exerciseResponse.Content.ReadFromJsonAsync<ExerciseResponse>();

        var addExerciseRequest = new AddWorkoutPlanExerciseRequest
        {
            ExerciseId = exercise!.Id,
            Order = 1,
            TargetSets = 3,
            TargetReps = 10,
            TargetWeight = 60m
        };

        var addExerciseResponse = await client.PostAsJsonAsync($"/api/workout-plans/{plan!.Id}/exercises", addExerciseRequest);

        Assert.Equal(HttpStatusCode.Created, addExerciseResponse.StatusCode);
        var planExercise = await addExerciseResponse.Content.ReadFromJsonAsync<WorkoutPlanExerciseResponse>();

        Assert.Equal(addExerciseRequest.ExerciseId, planExercise!.ExerciseId);
        Assert.Equal(addExerciseRequest.Order, planExercise.Order);
        Assert.Equal(addExerciseRequest.TargetSets, planExercise.TargetSets);
        Assert.Equal(addExerciseRequest.TargetReps, planExercise.TargetReps);
        Assert.Equal(addExerciseRequest.TargetWeight, planExercise.TargetWeight);
    }

    [Fact]
    public async Task AddExercise_with_duplicate_exercise_id_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var planResponse = await client.PostAsJsonAsync("/api/workout-plans", new CreateWorkoutPlanRequest
        {
            Name = "Pull Day"
        });
        var plan = await planResponse.Content.ReadFromJsonAsync<WorkoutPlanResponse>();

        var exerciseResponse = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseRequest
        {
            Name = "Pull Up",
            MuscleGroup = "Back",
            Description = "Bodyweight pull up"
        });
        var exercise = await exerciseResponse.Content.ReadFromJsonAsync<ExerciseResponse>();

        var addExerciseRequest = new AddWorkoutPlanExerciseRequest
        {
            ExerciseId = exercise!.Id,
            Order = 1,
            TargetSets = 3,
            TargetReps = 8,
            TargetWeight = 0m
        };

        var firstAddResponse = await client.PostAsJsonAsync($"/api/workout-plans/{plan!.Id}/exercises", addExerciseRequest);
        Assert.Equal(HttpStatusCode.Created, firstAddResponse.StatusCode);

        var duplicateAddRequest = addExerciseRequest with { Order = 2 };
        var duplicateResponse = await client.PostAsJsonAsync($"/api/workout-plans/{plan.Id}/exercises", duplicateAddRequest);

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task AddExercise_with_zero_target_sets_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var planResponse = await client.PostAsJsonAsync("/api/workout-plans", new CreateWorkoutPlanRequest
        {
            Name = "Zero Sets Day"
        });
        var plan = await planResponse.Content.ReadFromJsonAsync<WorkoutPlanResponse>();

        var exerciseResponse = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseRequest
        {
            Name = "Squat",
            MuscleGroup = "Legs",
            Description = "Barbell squat"
        });
        var exercise = await exerciseResponse.Content.ReadFromJsonAsync<ExerciseResponse>();

        var addExerciseRequest = new AddWorkoutPlanExerciseRequest
        {
            ExerciseId = exercise!.Id,
            Order = 1,
            TargetSets = 0,
            TargetReps = 10,
            TargetWeight = 40m
        };

        var addExerciseResponse = await client.PostAsJsonAsync($"/api/workout-plans/{plan!.Id}/exercises", addExerciseRequest);

        Assert.Equal(HttpStatusCode.BadRequest, addExerciseResponse.StatusCode);
    }

    [Fact]
    public async Task AddExercise_with_nonexistent_exercise_id_returns_not_found()
    {
        var client = await CreateAuthenticatedClientAsync();

        var planResponse = await client.PostAsJsonAsync("/api/workout-plans", new CreateWorkoutPlanRequest
        {
            Name = "Leg Day"
        });
        var plan = await planResponse.Content.ReadFromJsonAsync<WorkoutPlanResponse>();

        var addExerciseRequest = new AddWorkoutPlanExerciseRequest
        {
            ExerciseId = 999_999,
            Order = 1,
            TargetSets = 3,
            TargetReps = 10,
            TargetWeight = 20m
        };

        var addExerciseResponse = await client.PostAsJsonAsync($"/api/workout-plans/{plan!.Id}/exercises", addExerciseRequest);

        Assert.Equal(HttpStatusCode.NotFound, addExerciseResponse.StatusCode);
    }

    [Fact]
    public async Task GetWorkoutPlan_owned_by_other_user_returns_not_found()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var planResponse = await ownerClient.PostAsJsonAsync("/api/workout-plans", new CreateWorkoutPlanRequest
        {
            Name = "Owner's Plan"
        });
        var plan = await planResponse.Content.ReadFromJsonAsync<WorkoutPlanResponse>();

        var otherClient = await CreateAuthenticatedClientAsync();
        var response = await otherClient.GetAsync($"/api/workout-plans/{plan!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExercise_in_use_by_workout_log_entry_returns_conflict()
    {
        var client = await CreateAuthenticatedClientAsync();

        var exerciseResponse = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseRequest
        {
            Name = "Barbell Row",
            MuscleGroup = "Back",
            Description = "Bent over barbell row"
        });
        var exercise = await exerciseResponse.Content.ReadFromJsonAsync<ExerciseResponse>();

        var logResponse = await client.PostAsJsonAsync("/api/workout-logs", new CreateWorkoutLogRequest
        {
            Date = DateTime.UtcNow.AddDays(-1),
            Notes = "Session",
            WorkoutPlanId = null
        });
        var log = await logResponse.Content.ReadFromJsonAsync<WorkoutLogResponse>();

        var addEntryResponse = await client.PostAsJsonAsync($"/api/workout-logs/{log!.Id}/entries", new AddWorkoutLogEntryRequest
        {
            ExerciseId = exercise!.Id,
            SetsCompleted = 3,
            RepsCompleted = 10,
            WeightUsed = 40m
        });
        Assert.Equal(HttpStatusCode.Created, addEntryResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/exercises/{exercise.Id}");

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteExercise_in_use_by_workout_plan_returns_conflict_and_leaves_exercise_intact()
    {
        var client = await CreateAuthenticatedClientAsync();

        var exerciseResponse = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseRequest
        {
            Name = "Incline Press",
            MuscleGroup = "Chest",
            Description = "Incline barbell press"
        });
        var exercise = await exerciseResponse.Content.ReadFromJsonAsync<ExerciseResponse>();

        var planResponse = await client.PostAsJsonAsync("/api/workout-plans", new CreateWorkoutPlanRequest
        {
            Name = "Chest Day"
        });
        var plan = await planResponse.Content.ReadFromJsonAsync<WorkoutPlanResponse>();

        var addExerciseResponse = await client.PostAsJsonAsync($"/api/workout-plans/{plan!.Id}/exercises", new AddWorkoutPlanExerciseRequest
        {
            ExerciseId = exercise!.Id,
            Order = 1,
            TargetSets = 3,
            TargetReps = 10,
            TargetWeight = 30m
        });
        Assert.Equal(HttpStatusCode.Created, addExerciseResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/exercises/{exercise.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/exercises");
        var exercises = await getResponse.Content.ReadFromJsonAsync<List<ExerciseResponse>>();
        Assert.Contains(exercises!, e => e.Id == exercise.Id);
    }
}
