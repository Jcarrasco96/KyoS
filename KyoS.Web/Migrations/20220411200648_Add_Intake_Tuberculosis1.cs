using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Add_Intake_Tuberculosis1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Education",
                table: "IntakeTuberculosis");

            migrationBuilder.DropColumn(
                name: "TheAbove",
                table: "IntakeTuberculosis");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Education",
                table: "IntakeTuberculosis",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TheAbove",
                table: "IntakeTuberculosis",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
