using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class DiagnosticTempForClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdClient",
                table: "ReferredsTemp",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdClient",
                table: "HealthInsuranceTemp",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdClient",
                table: "DocumentsTemp",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdClient",
                table: "DiagnosticsTemp",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdClient",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "IdClient",
                table: "HealthInsuranceTemp");

            migrationBuilder.DropColumn(
                name: "IdClient",
                table: "DocumentsTemp");

            migrationBuilder.DropColumn(
                name: "IdClient",
                table: "DiagnosticsTemp");
        }
    }
}
