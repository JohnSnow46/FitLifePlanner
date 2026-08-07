using FitLifePlanner.Api.Common;
using FitLifePlanner.Api.Contracts.Progress;
using FitLifePlanner.Domain.Common;
using FitLifePlanner.Domain.Progress;
using FitLifePlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitLifePlanner.Api.Controllers;

[ApiController]
[Route("api")]
public class MealLogsController(FitLifePlannerDbContext context) : ControllerBase
{
    [HttpGet("meal-logs")]
    public async Task<ActionResult<IReadOnlyCollection<MealLogResponse>>> GetMealLogsAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userId = User.GetUserId();

        var query = context.MealLogs.Where(l => l.UserId == userId);

        if (from is not null)
        {
            query = query.Where(l => l.Date >= from);
        }

        if (to is not null)
        {
            query = query.Where(l => l.Date <= to);
        }

        var logs = await query.ToListAsync();

        return Ok(logs.Select(l => l.ToResponse()).ToList());
    }

    [HttpGet("meal-logs/{id:int}")]
    public async Task<ActionResult<MealLogResponse>> GetMealLogAsync(int id)
    {
        var userId = User.GetUserId();

        var log = await context.MealLogs
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId)
            ?? throw new NotFoundException("MealLog", id);

        return Ok(log.ToResponse());
    }

    [HttpPost("meal-logs")]
    public async Task<ActionResult<MealLogResponse>> CreateMealLogAsync(CreateMealLogRequest request)
    {
        var userId = User.GetUserId();

        var log = MealLog.Create(userId, request.Date, request.MealType, request.FoodId, request.QuantityConsumed);

        context.MealLogs.Add(log);
        await context.SaveChangesAsync();

        return Created($"/api/meal-logs/{log.Id}", log.ToResponse());
    }

    [HttpDelete("meal-logs/{id:int}")]
    public async Task<IActionResult> DeleteMealLogAsync(int id)
    {
        var userId = User.GetUserId();

        var log = await context.MealLogs
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId)
            ?? throw new NotFoundException("MealLog", id);

        context.MealLogs.Remove(log);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
