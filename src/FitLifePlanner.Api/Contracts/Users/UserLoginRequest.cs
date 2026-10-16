using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Users;

public record UserLoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Password { get; init; } = string.Empty;
}
