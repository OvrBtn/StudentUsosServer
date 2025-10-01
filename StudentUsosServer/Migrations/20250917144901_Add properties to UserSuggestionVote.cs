using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentUsosServer.Migrations
{
    /// <inheritdoc />
    public partial class AddpropertiestoUserSuggestionVote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InternalRoomInfoId",
                table: "UserSuggestionVotes",
                newName: "InternalUserSuggestionId");

            migrationBuilder.AddColumn<string>(
                name: "UserInstallation",
                table: "UserSuggestionVotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserStudentNumber",
                table: "UserSuggestionVotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserInstallation",
                table: "UserSuggestionVotes");

            migrationBuilder.DropColumn(
                name: "UserStudentNumber",
                table: "UserSuggestionVotes");

            migrationBuilder.RenameColumn(
                name: "InternalUserSuggestionId",
                table: "UserSuggestionVotes",
                newName: "InternalRoomInfoId");
        }
    }
}
