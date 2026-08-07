using FitLifePlanner.Domain.Workouts;

namespace FitLifePlanner.Api.Contracts.Workouts;

public static class WorkoutsMappings
{
    public static ExerciseResponse ToResponse(this Exercise exercise) =>
        new(exercise.Id, exercise.Name, exercise.MuscleGroup, exercise.Description);

    public static WorkoutPlanResponse ToResponse(this WorkoutPlan plan) =>
        new(plan.Id, plan.Name, plan.CreatedAt);

    public static WorkoutPlanDetailResponse ToDetailResponse(this WorkoutPlan plan) =>
        new(plan.Id, plan.Name, plan.CreatedAt, plan.Exercises.Select(e => e.ToResponse()).ToList());

    public static WorkoutPlanExerciseResponse ToResponse(this WorkoutPlanExercise exercise) =>
        new(exercise.Id, exercise.ExerciseId, exercise.Order, exercise.TargetSets, exercise.TargetReps, exercise.TargetWeight);
}
