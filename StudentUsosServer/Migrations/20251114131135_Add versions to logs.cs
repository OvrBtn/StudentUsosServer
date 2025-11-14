using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentUsosServer.Migrations
{
    /// <inheritdoc />
    public partial class Addversionstologs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiVersion",
                table: "AppLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppVersion",
                table: "AppLogs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiVersion",
                table: "AppLogs");

            migrationBuilder.DropColumn(
                name: "AppVersion",
                table: "AppLogs");
        }
    }
}
