using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class updateTCMClient1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "TCMClient");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "TCMClient",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "TCMClient");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "TCMClient",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
