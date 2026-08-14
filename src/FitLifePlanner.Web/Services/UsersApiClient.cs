using System.Net.Http.Json;
using FitLifePlanner.Web.Contracts.Users;

namespace FitLifePlanner.Web.Services;

public class UsersApiClient(HttpClient httpClient)
{
    public async Task<AuthResponse> RegisterAsync(UserRegisterRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/auth/register", request, cancellationToken);
        return await ReadResponseAsync<AuthResponse>(response, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        return await ReadResponseAsync<AuthResponse>(response, cancellationToken);
    }

    public async Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("api/users/me", cancellationToken);
        return await ReadResponseAsync<UserResponse>(response, cancellationToken);
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
