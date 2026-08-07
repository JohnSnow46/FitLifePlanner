using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
}
