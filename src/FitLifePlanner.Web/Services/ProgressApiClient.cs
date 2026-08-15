using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FitLifePlanner.Web.Contracts.Progress;

namespace FitLifePlanner.Web.Services;

public class ProgressApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<IReadOnlyCollection<WorkoutLogResponse>> GetWorkoutLogsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var url = BuildDateRangeUrl("api/workout-logs", from, to);
        var response = await httpClient.GetAsync(url, cancellationToken);
        return await ReadResponseAsync<IReadOnlyCollection<WorkoutLogResponse>>(response, cancellationToken);
    }

    public async Task<WorkoutLogDetailResponse> GetWorkoutLogAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/workout-logs/{id}", cancellationToken);
        return await ReadResponseAsync<WorkoutLogDetailResponse>(response, cancellationToken);
    }

    public async Task<WorkoutLogResponse> CreateWorkoutLogAsync(CreateWorkoutLogRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/workout-logs", request, cancellationToken);
        return await ReadResponseAsync<WorkoutLogResponse>(response, cancellationToken);
    }

    public async Task DeleteWorkoutLogAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/workout-logs/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<WorkoutLogEntryResponse> AddWorkoutLogEntryAsync(int logId, AddWorkoutLogEntryRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/workout-logs/{logId}/entries", request, cancellationToken);
        return await ReadResponseAsync<WorkoutLogEntryResponse>(response, cancellationToken);
    }

    public async Task RemoveWorkoutLogEntryAsync(int logId, int entryId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/workout-logs/{logId}/entries/{entryId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MealLogResponse>> GetMealLogsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var url = BuildDateRangeUrl("api/meal-logs", from, to);
        var response = await httpClient.GetAsync(url, cancellationToken);
        return await ReadResponseAsync<IReadOnlyCollection<MealLogResponse>>(response, cancellationToken);
    }

    public async Task<MealLogResponse> GetMealLogAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/meal-logs/{id}", cancellationToken);
        return await ReadResponseAsync<MealLogResponse>(response, cancellationToken);
    }

    public async Task<MealLogResponse> CreateMealLogAsync(CreateMealLogRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/meal-logs", request, JsonOptions, cancellationToken);
        return await ReadResponseAsync<MealLogResponse>(response, cancellationToken);
    }

    public async Task DeleteMealLogAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/meal-logs/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyCollection<BodyMetricEntryResponse>> GetBodyMetricEntriesAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var url = BuildDateRangeUrl("api/body-metrics", from, to);
        var response = await httpClient.GetAsync(url, cancellationToken);
        return await ReadResponseAsync<IReadOnlyCollection<BodyMetricEntryResponse>>(response, cancellationToken);
    }

    public async Task<BodyMetricEntryResponse> GetBodyMetricEntryAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/body-metrics/{id}", cancellationToken);
        return await ReadResponseAsync<BodyMetricEntryResponse>(response, cancellationToken);
    }

    public async Task<BodyMetricEntryResponse> CreateBodyMetricEntryAsync(CreateBodyMetricEntryRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/body-metrics", request, cancellationToken);
        return await ReadResponseAsync<BodyMetricEntryResponse>(response, cancellationToken);
    }

    public async Task DeleteBodyMetricEntryAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/body-metrics/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static string BuildDateRangeUrl(string basePath, DateTime? from, DateTime? to)
    {
        var query = new List<string>();
        if (from is not null)
        {
            query.Add($"from={Uri.EscapeDataString(from.Value.ToString("o"))}");
        }

        if (to is not null)
        {
            query.Add($"to={Uri.EscapeDataString(to.Value.ToString("o"))}");
        }

        return query.Count == 0 ? basePath : $"{basePath}?{string.Join("&", query)}";
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
