using FitLifePlanner.Api.Common;
using FitLifePlanner.Api.Contracts.Users;
using FitLifePlanner.Api.Services;
using FitLifePlanner.Domain.Common;
using FitLifePlanner.Domain.Users;
using FitLifePlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitLifePlanner.Api.Controllers;

[ApiController]
[Route("api")]
public class UsersController(
    FitLifePlannerDbContext context,
    PasswordHasher<User> passwordHasher,
    TokenService tokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("auth/register")]
    public async Task<ActionResult<AuthResponse>> RegisterAsync(UserRegisterRequest request)
    {
        var emailTaken = await context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
        {
            throw new ValidationException("Email is already registered.");
        }

        var user = new User { Name = request.Name, Email = request.Email };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token));
    }

    [AllowAnonymous]
    [HttpPost("auth/login")]
    public async Task<ActionResult<AuthResponse>> LoginAsync(UserLoginRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token));
    }

    [HttpGet("users/me")]
    public async Task<ActionResult<UserResponse>> GetMeAsync()
    {
        var userId = User.GetUserId();

        var user = await context.Users.FindAsync(userId)
            ?? throw new NotFoundException("User", userId);

        return Ok(user.ToResponse());
    }

    [HttpPut("users/me")]
    public async Task<IActionResult> UpdateMeAsync(UserUpdateRequest request)
    {
        var userId = User.GetUserId();

        var user = await context.Users.FindAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (request.Email != user.Email)
        {
            var emailTaken = await context.Users.AnyAsync(u => u.Email == request.Email);
            if (emailTaken)
            {
                throw new ValidationException("Email is already registered.");
            }
        }

        user.Name = request.Name;
        user.Email = request.Email;

        await context.SaveChangesAsync();

        return NoContent();
    }
}
