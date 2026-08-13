using Microsoft.JSInterop;

namespace FitLifePlanner.Web.Services;

public class TokenStore(IJSRuntime jsRuntime)
{
    private const string TokenKey = "fitlife.token";

    private bool _loaded;
    private string? _cachedToken;

    public async Task<string?> GetTokenAsync()
    {
        if (_loaded)
        {
            return _cachedToken;
        }

        _cachedToken = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        _loaded = true;
        return _cachedToken;
    }

    public async Task SetTokenAsync(string token)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        _cachedToken = token;
        _loaded = true;
    }

    public async Task ClearTokenAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        _cachedToken = null;
        _loaded = true;
    }
}
