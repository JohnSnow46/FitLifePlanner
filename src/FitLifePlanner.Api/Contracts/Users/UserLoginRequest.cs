using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Users;

public record UserLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
