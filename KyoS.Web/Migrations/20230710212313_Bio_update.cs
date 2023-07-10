using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Bio_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AnyEating",
                table: "Bio",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AnyFood",
                table: "Bio",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MilitaryServiceHistory",
                table: "Bio",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MilitaryServiceHistory_Explain",
                table: "Bio",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VocationalAssesment",
                table: "Bio",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnyEating",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "AnyFood",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "MilitaryServiceHistory",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "MilitaryServiceHistory_Explain",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "VocationalAssesment",
                table: "Bio");
        }
    }
}
