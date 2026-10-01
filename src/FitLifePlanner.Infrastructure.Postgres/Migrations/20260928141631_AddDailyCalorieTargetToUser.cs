using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitLifePlanner.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyCalorieTargetToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailyCalorieTarget",
                table: "Users",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyCalorieTarget",
                table: "Users");
        }
    }
}
