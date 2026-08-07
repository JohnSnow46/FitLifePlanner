using FitLifePlanner.Api.Common;
using FitLifePlanner.Api.Contracts.Nutrition;
using FitLifePlanner.Domain.Common;
using FitLifePlanner.Domain.Nutrition;
using FitLifePlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitLifePlanner.Api.Controllers;

[ApiController]
[Route("api")]
public class NutritionController(FitLifePlannerDbContext context) : ControllerBase
{
    [HttpGet("foods")]
    public async Task<ActionResult<IReadOnlyCollection<FoodResponse>>> GetFoodsAsync()
    {
        var foods = await context.Foods.ToListAsync();

        return Ok(foods.Select(f => f.ToResponse()).ToList());
    }

    [HttpGet("foods/{id:int}")]
    public async Task<ActionResult<FoodResponse>> GetFoodAsync(int id)
    {
        var food = await context.Foods.FindAsync(id)
            ?? throw new NotFoundException("Food", id);

        return Ok(food.ToResponse());
    }

    [HttpPost("foods")]
    public async Task<ActionResult<FoodResponse>> CreateFoodAsync(CreateFoodRequest request)
    {
        var food = new Food
        {
            Name = request.Name,
            Unit = request.Unit,
            CaloriesPerUnit = request.CaloriesPerUnit,
            ProteinPerUnit = request.ProteinPerUnit,
            CarbsPerUnit = request.CarbsPerUnit,
            FatPerUnit = request.FatPerUnit
        };

        context.Foods.Add(food);
        await context.SaveChangesAsync();

        return Created($"/api/foods/{food.Id}", food.ToResponse());
    }

    [HttpPut("foods/{id:int}")]
    public async Task<IActionResult> UpdateFoodAsync(int id, UpdateFoodRequest request)
    {
        var food = await context.Foods.FindAsync(id)
            ?? throw new NotFoundException("Food", id);

        food.Name = request.Name;
        food.Unit = request.Unit;
        food.CaloriesPerUnit = request.CaloriesPerUnit;
        food.ProteinPerUnit = request.ProteinPerUnit;
        food.CarbsPerUnit = request.CarbsPerUnit;
        food.FatPerUnit = request.FatPerUnit;

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("foods/{id:int}")]
    public async Task<IActionResult> DeleteFoodAsync(int id)
    {
        var food = await context.Foods.FindAsync(id)
            ?? throw new NotFoundException("Food", id);

        context.Foods.Remove(food);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("meal-plans")]
    public async Task<ActionResult<IReadOnlyCollection<MealPlanResponse>>> GetMealPlansAsync()
    {
        var userId = User.GetUserId();

        var plans = await context.MealPlans
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return Ok(plans.Select(p => p.ToResponse()).ToList());
    }

    [HttpGet("meal-plans/{id:int}")]
    public async Task<ActionResult<MealPlanDetailResponse>> GetMealPlanAsync(int id)
    {
        var userId = User.GetUserId();

        var plan = await context.MealPlans
            .Include(p => p.Entries)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId)
            ?? throw new NotFoundException("MealPlan", id);

        return Ok(plan.ToDetailResponse());
    }

    [HttpPost("meal-plans")]
    public async Task<ActionResult<MealPlanResponse>> CreateMealPlanAsync(CreateMealPlanRequest request)
    {
        var userId = User.GetUserId();

        var plan = new MealPlan
        {
            UserId = userId,
            Name = request.Name
        };

        context.MealPlans.Add(plan);
        await context.SaveChangesAsync();

        return Created($"/api/meal-plans/{plan.Id}", plan.ToResponse());
    }

    [HttpPut("meal-plans/{id:int}")]
    public async Task<IActionResult> UpdateMealPlanAsync(int id, UpdateMealPlanRequest request)
    {
        var userId = User.GetUserId();

        var plan = await context.MealPlans
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId)
            ?? throw new NotFoundException("MealPlan", id);

        plan.Name = request.Name;

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("meal-plans/{id:int}")]
    public async Task<IActionResult> DeleteMealPlanAsync(int id)
    {
        var userId = User.GetUserId();

        var plan = await context.MealPlans
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId)
            ?? throw new NotFoundException("MealPlan", id);

        context.MealPlans.Remove(plan);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("meal-plans/{planId:int}/entries")]
    public async Task<ActionResult<MealPlanEntryResponse>> AddMealPlanEntryAsync(int planId, AddMealPlanEntryRequest request)
    {
        var userId = User.GetUserId();

        var plan = await context.MealPlans
            .Include(p => p.Entries)
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId)
            ?? throw new NotFoundException("MealPlan", planId);

        var entry = plan.AddEntry(request.FoodId, request.MealType, request.Quantity, request.DayOfWeek);

        await context.SaveChangesAsync();

        return Created($"/api/meal-plans/{planId}/entries/{entry.Id}", entry.ToResponse());
    }

    [HttpDelete("meal-plans/{planId:int}/entries/{entryId:int}")]
    public async Task<IActionResult> RemoveMealPlanEntryAsync(int planId, int entryId)
    {
        var userId = User.GetUserId();

        var planExists = await context.MealPlans
            .AnyAsync(p => p.Id == planId && p.UserId == userId);

        if (!planExists)
        {
            throw new NotFoundException("MealPlan", planId);
        }

        var entry = await context.MealPlanEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.MealPlanId == planId)
            ?? throw new NotFoundException("MealPlanEntry", entryId);

        context.MealPlanEntries.Remove(entry);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
