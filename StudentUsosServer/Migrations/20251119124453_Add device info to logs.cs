using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentUsosServer.Migrations
{
    /// <inheritdoc />
    public partial class Adddeviceinfotologs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceInfo",
                table: "AppLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceInfo",
                table: "AppLogs");
        }
    }
}
