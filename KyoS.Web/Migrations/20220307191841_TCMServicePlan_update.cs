using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMServicePlan_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "TCMServicePlans");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TCMServicePlans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TCMServicePlans");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "TCMServicePlans",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
