using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyWorkdayActivityFacilitators1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Service",
                table: "Workdays_Activities_Facilitators");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Service",
                table: "Workdays_Activities_Facilitators",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
