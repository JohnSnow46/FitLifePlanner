namespace FitLifePlanner.Web.Contracts.Users;

public record UserRegisterRequest
{
    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
