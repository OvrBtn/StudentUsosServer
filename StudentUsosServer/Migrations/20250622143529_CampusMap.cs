using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentUsosServer.Migrations
{
    /// <inheritdoc />
    public partial class CampusMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomInfos",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildingId = table.Column<string>(type: "TEXT", nullable: false),
                    Floor = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameWeight = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomInfos", x => x.RoomId);
                });

            migrationBuilder.CreateTable(
                name: "UserRoomInfoSuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserStudentNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    UserInstallation = table.Column<string>(type: "TEXT", nullable: false),
                    BuildingId = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    RoomId = table.Column<int>(type: "INTEGER", maxLength: 10, nullable: false),
                    Floor = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    SuggestedRoomName = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    SuggestionWeight = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoomInfoSuggestions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomInfos");

            migrationBuilder.DropTable(
                name: "UserRoomInfoSuggestions");
        }
    }
}
