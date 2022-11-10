using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class DiferentActivitiesForSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AM",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PM",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AM",
                table: "Workdays_Activities_Facilitators");

            migrationBuilder.DropColumn(
                name: "PM",
                table: "Workdays_Activities_Facilitators");
        }
    }
}
