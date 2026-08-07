using FitLifePlanner.Domain.Users;

namespace FitLifePlanner.Api.Contracts.Users;

public static class UsersMappings
{
    public static UserResponse ToResponse(this User user) => new(user.Id, user.Name, user.Email);
}
