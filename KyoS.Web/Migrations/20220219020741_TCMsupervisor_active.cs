using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMsupervisor_active : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TCMActive",
                table: "TCMsupervisors",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TCMActive",
                table: "TCMsupervisors");
        }
    }
}
