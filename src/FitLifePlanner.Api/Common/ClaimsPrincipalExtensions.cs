using System.Security.Claims;

namespace FitLifePlanner.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User id claim is missing from the current principal.");

        return int.Parse(claim.Value);
    }
}
