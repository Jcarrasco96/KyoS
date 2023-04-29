using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AuthorizationNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorizationNumber",
                table: "HealthInsuranceTemp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizationNumber",
                table: "Clients_HealthInsurances",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizationNumber",
                table: "HealthInsuranceTemp");

            migrationBuilder.DropColumn(
                name: "AuthorizationNumber",
                table: "Clients_HealthInsurances");
        }
    }
}
