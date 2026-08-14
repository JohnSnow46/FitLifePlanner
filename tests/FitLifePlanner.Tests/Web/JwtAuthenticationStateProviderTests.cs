using System.Security.Claims;
using System.Text;
using FitLifePlanner.Web.Services;

namespace FitLifePlanner.Tests.Web;

public class JwtAuthenticationStateProviderTests
{
    private const string TokenKey = "fitlife.token";

    [Fact]
    public async Task GetAuthenticationStateAsync_with_valid_token_returns_authenticated_principal_with_claims()
    {
        var jsRuntime = new FakeJsRuntime();
        var tokenStore = new TokenStore(jsRuntime);
        var provider = new JwtAuthenticationStateProvider(tokenStore);
        var token = BuildJwt("42", "user@example.com", DateTimeOffset.UtcNow.AddHours(1));
        jsRuntime.Storage[TokenKey] = token;

        var result = await provider.GetAuthenticationStateAsync();

        Assert.True(result.User.Identity!.IsAuthenticated);
        Assert.Equal("42", result.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Assert.Equal("user@example.com", result.User.FindFirst(ClaimTypes.Email)!.Value);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_with_expired_token_returns_anonymous_and_clears_token()
    {
        var jsRuntime = new FakeJsRuntime();
        var tokenStore = new TokenStore(jsRuntime);
        var provider = new JwtAuthenticationStateProvider(tokenStore);
        var token = BuildJwt("42", "user@example.com", DateTimeOffset.UtcNow.AddHours(-1));
        jsRuntime.Storage[TokenKey] = token;

        var result = await provider.GetAuthenticationStateAsync();

        Assert.False(result.User.Identity!.IsAuthenticated);
        Assert.False(jsRuntime.Storage.ContainsKey(TokenKey));
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_with_malformed_token_returns_anonymous()
    {
        var jsRuntime = new FakeJsRuntime();
        var tokenStore = new TokenStore(jsRuntime);
        var provider = new JwtAuthenticationStateProvider(tokenStore);
        jsRuntime.Storage[TokenKey] = "not-a-jwt";

        var result = await provider.GetAuthenticationStateAsync();

        Assert.False(result.User.Identity!.IsAuthenticated);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_with_no_token_returns_anonymous()
    {
        var jsRuntime = new FakeJsRuntime();
        var tokenStore = new TokenStore(jsRuntime);
        var provider = new JwtAuthenticationStateProvider(tokenStore);

        var result = await provider.GetAuthenticationStateAsync();

        Assert.False(result.User.Identity!.IsAuthenticated);
    }

    private static string BuildJwt(string sub, string email, DateTimeOffset exp)
    {
        var header = EncodeBase64Url("""{"alg":"none","typ":"JWT"}""");
        var payload = EncodeBase64Url($$"""{"sub":"{{sub}}","email":"{{email}}","exp":{{exp.ToUnixTimeSeconds()}}}""");
        return $"{header}.{payload}.sig";
    }

    private static string EncodeBase64Url(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
