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
public class BodyMetricEntriesController(FitLifePlannerDbContext context) : ControllerBase
{
    [HttpGet("body-metrics")]
    public async Task<ActionResult<IReadOnlyCollection<BodyMetricEntryResponse>>> GetBodyMetricEntriesAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userId = User.GetUserId();

        var query = context.BodyMetricEntries.Where(e => e.UserId == userId);

        if (from is not null)
        {
            query = query.Where(e => e.Date >= from);
        }

        if (to is not null)
        {
            query = query.Where(e => e.Date <= to);
        }

        var entries = await query.ToListAsync();

        return Ok(entries.Select(e => e.ToResponse()).ToList());
    }

    [HttpGet("body-metrics/{id:int}")]
    public async Task<ActionResult<BodyMetricEntryResponse>> GetBodyMetricEntryAsync(int id)
    {
        var userId = User.GetUserId();

        var entry = await context.BodyMetricEntries
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId)
            ?? throw new NotFoundException("BodyMetricEntry", id);

        return Ok(entry.ToResponse());
    }

    [HttpPost("body-metrics")]
    public async Task<ActionResult<BodyMetricEntryResponse>> CreateBodyMetricEntryAsync(CreateBodyMetricEntryRequest request)
    {
        var userId = User.GetUserId();

        var entry = BodyMetricEntry.Create(userId, request.Date, request.Weight, request.BodyFatPercent, request.Notes);

        context.BodyMetricEntries.Add(entry);
        await context.SaveChangesAsync();

        return Created($"/api/body-metrics/{entry.Id}", entry.ToResponse());
    }

    [HttpDelete("body-metrics/{id:int}")]
    public async Task<IActionResult> DeleteBodyMetricEntryAsync(int id)
    {
        var userId = User.GetUserId();

        var entry = await context.BodyMetricEntries
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId)
            ?? throw new NotFoundException("BodyMetricEntry", id);

        context.BodyMetricEntries.Remove(entry);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
