using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLifePlanner.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFavoriteToExerciseAndFood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Foods",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Exercises",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Exercises");
        }
    }
}
