using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Api.Contracts.Users;

public record UserUpdateRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}
