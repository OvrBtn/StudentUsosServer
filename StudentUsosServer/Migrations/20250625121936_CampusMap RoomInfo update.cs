using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentUsosServer.Migrations
{
    /// <inheritdoc />
    public partial class CampusMapRoomInfoupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RoomInfos",
                table: "RoomInfos");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "RoomInfos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "InternalId",
                table: "RoomInfos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoomInfos",
                table: "RoomInfos",
                column: "InternalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RoomInfos",
                table: "RoomInfos");

            migrationBuilder.DropColumn(
                name: "InternalId",
                table: "RoomInfos");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "RoomInfos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoomInfos",
                table: "RoomInfos",
                column: "RoomId");
        }
    }
}
