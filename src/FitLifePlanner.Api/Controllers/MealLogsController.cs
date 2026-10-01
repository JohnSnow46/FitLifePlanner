using System.Text;
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

    [HttpGet("meal-logs/summary")]
    public async Task<ActionResult<IReadOnlyCollection<NutritionDaySummaryResponse>>> GetNutritionSummaryAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userId = User.GetUserId();

        var rangeTo = (to ?? DateTime.UtcNow).Date;
        var rangeFrom = (from ?? rangeTo.AddDays(-6)).Date;

        var rows = await context.MealLogs
            .Where(l => l.UserId == userId && l.Date.Date >= rangeFrom && l.Date.Date <= rangeTo)
            .Join(context.Foods, l => l.FoodId, f => f.Id, (l, f) => new
            {
                l.Date,
                Calories = l.QuantityConsumed * f.CaloriesPerUnit,
                Protein = l.QuantityConsumed * f.ProteinPerUnit,
                Carbs = l.QuantityConsumed * f.CarbsPerUnit,
                Fat = l.QuantityConsumed * f.FatPerUnit
            })
            .ToListAsync();

        var summary = rows
            .GroupBy(x => x.Date.Date)
            .Select(g => new NutritionDaySummaryResponse(
                g.Key,
                g.Sum(x => x.Calories),
                g.Sum(x => x.Protein),
                g.Sum(x => x.Carbs),
                g.Sum(x => x.Fat)))
            .OrderBy(s => s.Date)
            .ToList();

        return Ok(summary);
    }

    [HttpGet("meal-logs/export")]
    public async Task<IActionResult> ExportMealLogsAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
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

        var logs = await query.OrderBy(l => l.Date).ToListAsync();

        var rows = logs.Select(l => (IReadOnlyList<string>)new[]
        {
            l.Date.ToString("yyyy-MM-dd"),
            l.MealType.ToString(),
            l.FoodId.ToString(),
            l.QuantityConsumed.ToString("0.##")
        });

        var csv = CsvExport.ToCsv(["Date", "MealType", "FoodId", "QuantityConsumed"], rows);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "meal-logs.csv");
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

        var foodExists = await context.Foods.AnyAsync(f => f.Id == request.FoodId);
        if (!foodExists)
        {
            throw new NotFoundException("Food", request.FoodId);
        }

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
