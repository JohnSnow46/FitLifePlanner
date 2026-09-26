namespace FitLifePlanner.Api.Contracts.Users;

public record UserResponse(int Id, string Name, string Email, decimal? TargetWeight, decimal? TargetBodyFatPercent);
