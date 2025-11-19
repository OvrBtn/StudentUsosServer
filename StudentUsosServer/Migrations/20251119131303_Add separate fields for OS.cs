using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentUsosServer.Migrations
{
    /// <inheritdoc />
    public partial class AddseparatefieldsforOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OperatingSystem",
                table: "AppLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingSystemVersion",
                table: "AppLogs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatingSystem",
                table: "AppLogs");

            migrationBuilder.DropColumn(
                name: "OperatingSystemVersion",
                table: "AppLogs");
        }
    }
}
