using System.Net;
using System.Net.Http.Json;
using FitLifePlanner.Api.Contracts.Users;

namespace FitLifePlanner.Tests.Api.Controllers;

public class UsersControllerTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
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
    public async Task GetMe_without_token_returns_unauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
