using System.Net.Http.Json;
using FitLifePlanner.Web.Contracts.Workouts;

namespace FitLifePlanner.Web.Services;

public class WorkoutsApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<ExerciseResponse>> GetExercisesAsync(string? muscleGroup = null, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrEmpty(muscleGroup)
            ? "api/exercises"
            : $"api/exercises?muscleGroup={Uri.EscapeDataString(muscleGroup)}";
        var response = await httpClient.GetAsync(url, cancellationToken);
        return await ReadResponseAsync<IReadOnlyCollection<ExerciseResponse>>(response, cancellationToken);
    }

    public async Task<ExerciseResponse> GetExerciseAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/exercises/{id}", cancellationToken);
        return await ReadResponseAsync<ExerciseResponse>(response, cancellationToken);
    }

    public async Task<ExerciseResponse> CreateExerciseAsync(CreateExerciseRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/exercises", request, cancellationToken);
        return await ReadResponseAsync<ExerciseResponse>(response, cancellationToken);
    }

    public async Task UpdateExerciseAsync(int id, UpdateExerciseRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/exercises/{id}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteExerciseAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/exercises/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyCollection<WorkoutPlanResponse>> GetWorkoutPlansAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("api/workout-plans", cancellationToken);
        return await ReadResponseAsync<IReadOnlyCollection<WorkoutPlanResponse>>(response, cancellationToken);
    }

    public async Task<WorkoutPlanDetailResponse> GetWorkoutPlanAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/workout-plans/{id}", cancellationToken);
        return await ReadResponseAsync<WorkoutPlanDetailResponse>(response, cancellationToken);
    }

    public async Task<WorkoutPlanResponse> CreateWorkoutPlanAsync(CreateWorkoutPlanRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/workout-plans", request, cancellationToken);
        return await ReadResponseAsync<WorkoutPlanResponse>(response, cancellationToken);
    }

    public async Task UpdateWorkoutPlanAsync(int id, UpdateWorkoutPlanRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/workout-plans/{id}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteWorkoutPlanAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/workout-plans/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<WorkoutPlanExerciseResponse> AddWorkoutPlanExerciseAsync(int planId, AddWorkoutPlanExerciseRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/workout-plans/{planId}/exercises", request, cancellationToken);
        return await ReadResponseAsync<WorkoutPlanExerciseResponse>(response, cancellationToken);
    }

    public async Task RemoveWorkoutPlanExerciseAsync(int planId, int planExerciseId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/workout-plans/{planId}/exercises/{planExerciseId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var message = await ExtractErrorMessageAsync(response, cancellationToken);
            throw new ApiException(response.StatusCode, message);
        }

        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
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
