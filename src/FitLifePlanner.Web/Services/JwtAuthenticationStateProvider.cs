using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace FitLifePlanner.Web.Services;

public class JwtAuthenticationStateProvider(TokenStore tokenStore) : AuthenticationStateProvider
{
    private static readonly AuthenticationState AnonymousState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStore.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return AnonymousState;
        }

        var claims = TryParseClaims(token);
        if (claims is null)
        {
            await tokenStore.ClearTokenAsync();
            return AnonymousState;
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task SignIn(string token)
    {
        await tokenStore.SetTokenAsync(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOut()
    {
        await tokenStore.ClearTokenAsync();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static List<Claim>? TryParseClaims(string token)
    {
        var segments = token.Split('.');
        if (segments.Length != 3)
        {
            return null;
        }

        Dictionary<string, JsonElement>? payload;
        try
        {
            var payloadJson = DecodeBase64Url(segments[1]);
            payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }

        if (payload is null || !payload.TryGetValue("exp", out var expElement))
        {
            return null;
        }

        if (!expElement.TryGetInt64(out var expSeconds))
        {
            return null;
        }

        var expiry = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
        if (expiry < DateTimeOffset.UtcNow)
        {
            return null;
        }

        var claims = new List<Claim>();
        if (payload.TryGetValue("sub", out var subElement))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, subElement.ToString()));
        }

        if (payload.TryGetValue("email", out var emailElement))
        {
            claims.Add(new Claim(ClaimTypes.Email, emailElement.ToString()));
        }

        return claims;
    }

    private static string DecodeBase64Url(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        var padding = base64.Length % 4;
        if (padding > 0)
        {
            base64 = base64.PadRight(base64.Length + (4 - padding), '=');
        }

        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }
}
