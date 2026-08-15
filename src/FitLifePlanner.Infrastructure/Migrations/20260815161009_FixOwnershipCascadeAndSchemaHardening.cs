using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLifePlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOwnershipCascadeAndSchemaHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealLogs_Foods_FoodId",
                table: "MealLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutLogEntries_Exercises_ExerciseId",
                table: "WorkoutLogEntries");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_UserId",
                table: "WorkoutPlans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_UserId",
                table: "WorkoutLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlans_UserId",
                table: "MealPlans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_UserId",
                table: "MealLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BodyMetricEntries_UserId",
                table: "BodyMetricEntries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MealLogs_Foods_FoodId",
                table: "MealLogs",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutLogEntries_Exercises_ExerciseId",
                table: "WorkoutLogEntries",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealLogs_Foods_FoodId",
                table: "MealLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutLogEntries_Exercises_ExerciseId",
                table: "WorkoutLogEntries");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutPlans_UserId",
                table: "WorkoutPlans");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutLogs_UserId",
                table: "WorkoutLogs");

            migrationBuilder.DropIndex(
                name: "IX_MealPlans_UserId",
                table: "MealPlans");

            migrationBuilder.DropIndex(
                name: "IX_MealLogs_UserId",
                table: "MealLogs");

            migrationBuilder.DropIndex(
                name: "IX_BodyMetricEntries_UserId",
                table: "BodyMetricEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_MealLogs_Foods_FoodId",
                table: "MealLogs",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutLogEntries_Exercises_ExerciseId",
                table: "WorkoutLogEntries",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
