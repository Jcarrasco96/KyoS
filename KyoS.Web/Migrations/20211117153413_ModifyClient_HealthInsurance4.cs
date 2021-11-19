using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyClient_HealthInsurance4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedUnits",
                table: "Clients_HealthInsurances");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsedUnits",
                table: "Clients_HealthInsurances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
