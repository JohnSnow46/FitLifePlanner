using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FitLifePlanner.Api.Contracts.Users;

namespace FitLifePlanner.Tests.Api.Controllers;

public class UsersControllerTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
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
    public async Task Register_with_new_email_returns_ok_with_token()
    {
        var client = factory.CreateClient();
        var request = new UserRegisterRequest
        {
            Name = "Jan Kowalski",
            Email = $"{Guid.NewGuid()}@example.com",
            Password = "correct-horse-battery"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body?.Token));
    }

    [Fact]
    public async Task Login_with_registered_credentials_returns_ok_with_token()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        var password = "correct-horse-battery";

        await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequest
        {
            Name = "Jan Kowalski",
            Email = email,
            Password = password
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new UserLoginRequest
        {
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body?.Token));
    }

    [Fact]
    public async Task Register_with_already_registered_email_returns_bad_request()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";

        var firstResponse = await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequest
        {
            Name = "Jan Kowalski",
            Email = email,
            Password = "correct-horse-battery"
        });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequest
        {
            Name = "Inny Jan",
            Email = email,
            Password = "another-password"
        });

        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_with_different_email_casing_returns_ok_with_token()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@Example.com";
        var password = "correct-horse-battery";

        await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequest
        {
            Name = "Jan Kowalski",
            Email = email,
            Password = password
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new UserLoginRequest
        {
            Email = email.ToUpperInvariant(),
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body?.Token));
    }

    [Fact]
    public async Task Register_with_already_registered_email_in_different_casing_returns_bad_request()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";

        var firstResponse = await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequest
        {
            Name = "Jan Kowalski",
            Email = email,
            Password = "correct-horse-battery"
        });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequest
        {
            Name = "Inny Jan",
            Email = email.ToUpperInvariant(),
            Password = "another-password"
        });

        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_with_overlong_email_returns_bad_request()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new UserLoginRequest
        {
            Email = $"{new string('a', 195)}@example.com",
            Password = "correct-horse-battery"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_with_overlong_password_returns_bad_request()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequest
        {
            Name = "Jan Kowalski",
            Email = $"{Guid.NewGuid()}@example.com",
            Password = new string('a', 101)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_overlong_password_returns_bad_request()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new UserLoginRequest
        {
            Email = $"{Guid.NewGuid()}@example.com",
            Password = new string('a', 101)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_without_token_returns_unauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGoals_with_valid_values_persists_them()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync("/api/users/me/goals", new UpdateBodyGoalsRequest
        {
            TargetWeight = 72.5m,
            TargetBodyFatPercent = 14m
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var me = await client.GetFromJsonAsync<UserResponse>("/api/users/me");
        Assert.Equal(72.5m, me!.TargetWeight);
        Assert.Equal(14m, me.TargetBodyFatPercent);
    }

    [Fact]
    public async Task UpdateGoals_with_non_positive_target_weight_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync("/api/users/me/goals", new UpdateBodyGoalsRequest
        {
            TargetWeight = 0m,
            TargetBodyFatPercent = null
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGoals_without_token_returns_unauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/users/me/goals", new UpdateBodyGoalsRequest
        {
            TargetWeight = 72.5m
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNutritionTarget_with_valid_value_persists_it()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync("/api/users/me/nutrition-target", new UpdateNutritionTargetRequest
        {
            DailyCalorieTarget = 2200m
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var me = await client.GetFromJsonAsync<UserResponse>("/api/users/me");
        Assert.Equal(2200m, me!.DailyCalorieTarget);
    }

    [Fact]
    public async Task UpdateNutritionTarget_with_non_positive_value_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync("/api/users/me/nutrition-target", new UpdateNutritionTargetRequest
        {
            DailyCalorieTarget = 0m
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNutritionTarget_without_token_returns_unauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/users/me/nutrition-target", new UpdateNutritionTargetRequest
        {
            DailyCalorieTarget = 2200m
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_with_correct_current_password_allows_login_with_the_new_one()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequest
        {
            Name = "Jan Kowalski",
            Email = email,
            Password = "correct-horse-battery"
        });
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerBody!.Token);

        var response = await client.PutAsJsonAsync("/api/users/me/password", new ChangePasswordRequest
        {
            CurrentPassword = "correct-horse-battery",
            NewPassword = "new-correct-horse-battery"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var loginResponse = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new UserLoginRequest
        {
            Email = email,
            Password = "new-correct-horse-battery"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_with_wrong_current_password_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync("/api/users/me/password", new ChangePasswordRequest
        {
            CurrentPassword = "not-the-real-password",
            NewPassword = "new-correct-horse-battery"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_with_too_short_new_password_returns_bad_request()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync("/api/users/me/password", new ChangePasswordRequest
        {
            CurrentPassword = "correct-horse-battery",
            NewPassword = "short"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_without_token_returns_unauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/users/me/password", new ChangePasswordRequest
        {
            CurrentPassword = "correct-horse-battery",
            NewPassword = "new-correct-horse-battery"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
