using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyNoteEntity5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Decompensating",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MinimalProgress",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ModerateProgress",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NoProgress",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Regression",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SignificantProgress",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UnableToDetermine",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Decompensating",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "MinimalProgress",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "ModerateProgress",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "NoProgress",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Regression",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "SignificantProgress",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "UnableToDetermine",
                table: "Notes");
        }
    }
}
