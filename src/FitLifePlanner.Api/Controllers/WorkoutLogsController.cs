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
public class WorkoutLogsController(FitLifePlannerDbContext context) : ControllerBase
{
    [HttpGet("workout-logs")]
    public async Task<ActionResult<IReadOnlyCollection<WorkoutLogResponse>>> GetWorkoutLogsAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userId = User.GetUserId();

        var query = context.WorkoutLogs.Where(l => l.UserId == userId);

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

    [HttpGet("workout-logs/{id:int}")]
    public async Task<ActionResult<WorkoutLogDetailResponse>> GetWorkoutLogAsync(int id)
    {
        var userId = User.GetUserId();

        var log = await context.WorkoutLogs
            .Include(l => l.Entries)
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId)
            ?? throw new NotFoundException("WorkoutLog", id);

        return Ok(log.ToDetailResponse());
    }

    [HttpPost("workout-logs")]
    public async Task<ActionResult<WorkoutLogResponse>> CreateWorkoutLogAsync(CreateWorkoutLogRequest request)
    {
        var userId = User.GetUserId();

        if (request.WorkoutPlanId is not null)
        {
            var planExists = await context.WorkoutPlans
                .AnyAsync(p => p.Id == request.WorkoutPlanId && p.UserId == userId);

            if (!planExists)
            {
                throw new NotFoundException("WorkoutPlan", request.WorkoutPlanId);
            }
        }

        var log = WorkoutLog.Create(userId, request.Date, request.Notes, request.WorkoutPlanId);

        context.WorkoutLogs.Add(log);
        await context.SaveChangesAsync();

        return Created($"/api/workout-logs/{log.Id}", log.ToResponse());
    }

    [HttpDelete("workout-logs/{id:int}")]
    public async Task<IActionResult> DeleteWorkoutLogAsync(int id)
    {
        var userId = User.GetUserId();

        var log = await context.WorkoutLogs
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId)
            ?? throw new NotFoundException("WorkoutLog", id);

        context.WorkoutLogs.Remove(log);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("workout-logs/{logId:int}/entries")]
    public async Task<ActionResult<WorkoutLogEntryResponse>> AddWorkoutLogEntryAsync(int logId, AddWorkoutLogEntryRequest request)
    {
        var userId = User.GetUserId();

        var log = await context.WorkoutLogs
            .Include(l => l.Entries)
            .FirstOrDefaultAsync(l => l.Id == logId && l.UserId == userId)
            ?? throw new NotFoundException("WorkoutLog", logId);

        var exerciseExists = await context.Exercises.AnyAsync(e => e.Id == request.ExerciseId);
        if (!exerciseExists)
        {
            throw new NotFoundException("Exercise", request.ExerciseId);
        }

        var entry = log.AddEntry(request.ExerciseId, request.SetsCompleted, request.RepsCompleted, request.WeightUsed);

        await context.SaveChangesAsync();

        return Created($"/api/workout-logs/{logId}/entries/{entry.Id}", entry.ToResponse());
    }

    [HttpDelete("workout-logs/{logId:int}/entries/{entryId:int}")]
    public async Task<IActionResult> RemoveWorkoutLogEntryAsync(int logId, int entryId)
    {
        var userId = User.GetUserId();

        var logExists = await context.WorkoutLogs
            .AnyAsync(l => l.Id == logId && l.UserId == userId);

        if (!logExists)
        {
            throw new NotFoundException("WorkoutLog", logId);
        }

        var entry = await context.WorkoutLogEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.WorkoutLogId == logId)
            ?? throw new NotFoundException("WorkoutLogEntry", entryId);

        context.WorkoutLogEntries.Remove(entry);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
