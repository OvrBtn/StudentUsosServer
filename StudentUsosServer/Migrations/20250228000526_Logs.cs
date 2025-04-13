using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentUsosServer.Migrations
{
    /// <inheritdoc />
    public partial class Logs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserInternalAccessToken",
                table: "AppLogs",
                newName: "UserUsosId");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "AppLogs",
                newName: "UserInstallation");

            migrationBuilder.RenameColumn(
                name: "Stacktrace",
                table: "AppLogs",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "AppLogs",
                newName: "LogLevel");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "AppLogs",
                newName: "ExceptionSerialized");

            migrationBuilder.AddColumn<string>(
                name: "CallerLineNumber",
                table: "AppLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CallerName",
                table: "AppLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreationDate",
                table: "AppLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreationDateUnix",
                table: "AppLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExceptionMessage",
                table: "AppLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallerLineNumber",
                table: "AppLogs");

            migrationBuilder.DropColumn(
                name: "CallerName",
                table: "AppLogs");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "AppLogs");

            migrationBuilder.DropColumn(
                name: "CreationDateUnix",
                table: "AppLogs");

            migrationBuilder.DropColumn(
                name: "ExceptionMessage",
                table: "AppLogs");

            migrationBuilder.RenameColumn(
                name: "UserUsosId",
                table: "AppLogs",
                newName: "UserInternalAccessToken");

            migrationBuilder.RenameColumn(
                name: "UserInstallation",
                table: "AppLogs",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "AppLogs",
                newName: "Stacktrace");

            migrationBuilder.RenameColumn(
                name: "LogLevel",
                table: "AppLogs",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "ExceptionSerialized",
                table: "AppLogs",
                newName: "Description");
        }
    }
}
