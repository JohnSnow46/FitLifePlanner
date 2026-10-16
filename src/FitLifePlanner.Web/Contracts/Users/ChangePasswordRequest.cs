using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Users;

public record ChangePasswordRequest
{
    [Required]
    [MaxLength(100)]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string NewPassword { get; init; } = string.Empty;
}
