using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLifePlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestrictPlanCatalogCascades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanEntries_Foods_FoodId",
                table: "MealPlanEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutPlanExercises_Exercises_ExerciseId",
                table: "WorkoutPlanExercises");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanEntries_Foods_FoodId",
                table: "MealPlanEntries",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutPlanExercises_Exercises_ExerciseId",
                table: "WorkoutPlanExercises",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanEntries_Foods_FoodId",
                table: "MealPlanEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutPlanExercises_Exercises_ExerciseId",
                table: "WorkoutPlanExercises");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanEntries_Foods_FoodId",
                table: "MealPlanEntries",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutPlanExercises_Exercises_ExerciseId",
                table: "WorkoutPlanExercises",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
