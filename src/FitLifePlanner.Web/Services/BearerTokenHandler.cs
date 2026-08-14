using System.Net;
using System.Net.Http.Headers;

namespace FitLifePlanner.Web.Services;

public class BearerTokenHandler(TokenStore tokenStore, JwtAuthenticationStateProvider authStateProvider)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenStore.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await authStateProvider.SignOut();
        }

        return response;
    }
}
