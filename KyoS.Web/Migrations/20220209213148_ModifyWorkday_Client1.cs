using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyWorkday_Client1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupSize",
                table: "Notes");

            migrationBuilder.AddColumn<int>(
                name: "GroupSize",
                table: "Workdays_Clients",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupSize",
                table: "Workdays_Clients");

            migrationBuilder.AddColumn<int>(
                name: "GroupSize",
                table: "Notes",
                type: "int",
                nullable: true);
        }
    }
}
