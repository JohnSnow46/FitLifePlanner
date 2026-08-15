using FitLifePlanner.Api.Common;
using FitLifePlanner.Api.Contracts.Workouts;
using FitLifePlanner.Domain.Common;
using FitLifePlanner.Domain.Workouts;
using FitLifePlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitLifePlanner.Api.Controllers;

[ApiController]
[Route("api")]
public class WorkoutsController(FitLifePlannerDbContext context) : ControllerBase
{
    [HttpGet("exercises")]
    public async Task<ActionResult<IReadOnlyCollection<ExerciseResponse>>> GetExercisesAsync([FromQuery] string? muscleGroup)
    {
        var query = context.Exercises.AsQueryable();

        if (!string.IsNullOrEmpty(muscleGroup))
        {
            query = query.Where(e => e.MuscleGroup == muscleGroup);
        }

        var exercises = await query.ToListAsync();

        return Ok(exercises.Select(e => e.ToResponse()).ToList());
    }

    [HttpGet("exercises/{id:int}")]
    public async Task<ActionResult<ExerciseResponse>> GetExerciseAsync(int id)
    {
        var exercise = await context.Exercises.FindAsync(id)
            ?? throw new NotFoundException("Exercise", id);

        return Ok(exercise.ToResponse());
    }

    [HttpPost("exercises")]
    public async Task<ActionResult<ExerciseResponse>> CreateExerciseAsync(CreateExerciseRequest request)
    {
        var exercise = new Exercise
        {
            Name = request.Name,
            MuscleGroup = request.MuscleGroup,
            Description = request.Description
        };

        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        return Created($"/api/exercises/{exercise.Id}", exercise.ToResponse());
    }

    [HttpPut("exercises/{id:int}")]
    public async Task<IActionResult> UpdateExerciseAsync(int id, UpdateExerciseRequest request)
    {
        var exercise = await context.Exercises.FindAsync(id)
            ?? throw new NotFoundException("Exercise", id);

        exercise.Name = request.Name;
        exercise.MuscleGroup = request.MuscleGroup;
        exercise.Description = request.Description;

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("exercises/{id:int}")]
    public async Task<IActionResult> DeleteExerciseAsync(int id)
    {
        var exercise = await context.Exercises.FindAsync(id)
            ?? throw new NotFoundException("Exercise", id);

        context.Exercises.Remove(exercise);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("workout-plans")]
    public async Task<ActionResult<IReadOnlyCollection<WorkoutPlanResponse>>> GetWorkoutPlansAsync()
    {
        var userId = User.GetUserId();

        var plans = await context.WorkoutPlans
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return Ok(plans.Select(p => p.ToResponse()).ToList());
    }

    [HttpGet("workout-plans/{id:int}")]
    public async Task<ActionResult<WorkoutPlanDetailResponse>> GetWorkoutPlanAsync(int id)
    {
        var userId = User.GetUserId();

        var plan = await context.WorkoutPlans
            .Include(p => p.Exercises)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId)
            ?? throw new NotFoundException("WorkoutPlan", id);

        return Ok(plan.ToDetailResponse());
    }

    [HttpPost("workout-plans")]
    public async Task<ActionResult<WorkoutPlanResponse>> CreateWorkoutPlanAsync(CreateWorkoutPlanRequest request)
    {
        var userId = User.GetUserId();

        var plan = new WorkoutPlan
        {
            UserId = userId,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        context.WorkoutPlans.Add(plan);
        await context.SaveChangesAsync();

        return Created($"/api/workout-plans/{plan.Id}", plan.ToResponse());
    }

    [HttpPut("workout-plans/{id:int}")]
    public async Task<IActionResult> UpdateWorkoutPlanAsync(int id, UpdateWorkoutPlanRequest request)
    {
        var userId = User.GetUserId();

        var plan = await context.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId)
            ?? throw new NotFoundException("WorkoutPlan", id);

        plan.Name = request.Name;

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("workout-plans/{id:int}")]
    public async Task<IActionResult> DeleteWorkoutPlanAsync(int id)
    {
        var userId = User.GetUserId();

        var plan = await context.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId)
            ?? throw new NotFoundException("WorkoutPlan", id);

        context.WorkoutPlans.Remove(plan);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("workout-plans/{planId:int}/exercises")]
    public async Task<ActionResult<WorkoutPlanExerciseResponse>> AddWorkoutPlanExerciseAsync(int planId, AddWorkoutPlanExerciseRequest request)
    {
        var userId = User.GetUserId();

        var plan = await context.WorkoutPlans
            .Include(p => p.Exercises)
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId)
            ?? throw new NotFoundException("WorkoutPlan", planId);

        var exerciseExists = await context.Exercises.AnyAsync(e => e.Id == request.ExerciseId);
        if (!exerciseExists)
        {
            throw new NotFoundException("Exercise", request.ExerciseId);
        }

        var planExercise = plan.AddExercise(request.ExerciseId, request.Order, request.TargetSets, request.TargetReps, request.TargetWeight);

        await context.SaveChangesAsync();

        return Created($"/api/workout-plans/{planId}/exercises/{planExercise.Id}", planExercise.ToResponse());
    }

    [HttpDelete("workout-plans/{planId:int}/exercises/{planExerciseId:int}")]
    public async Task<IActionResult> RemoveWorkoutPlanExerciseAsync(int planId, int planExerciseId)
    {
        var userId = User.GetUserId();

        var planExists = await context.WorkoutPlans
            .AnyAsync(p => p.Id == planId && p.UserId == userId);

        if (!planExists)
        {
            throw new NotFoundException("WorkoutPlan", planId);
        }

        var planExercise = await context.WorkoutPlanExercises
            .FirstOrDefaultAsync(e => e.Id == planExerciseId && e.WorkoutPlanId == planId)
            ?? throw new NotFoundException("WorkoutPlanExercise", planExerciseId);

        context.WorkoutPlanExercises.Remove(planExercise);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
