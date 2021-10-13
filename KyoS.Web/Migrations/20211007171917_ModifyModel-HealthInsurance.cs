using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyModelHealthInsurance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "HealthInsurances",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthInsurances_ClinicId",
                table: "HealthInsurances",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthInsurances_Clinics_ClinicId",
                table: "HealthInsurances",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthInsurances_Clinics_ClinicId",
                table: "HealthInsurances");

            migrationBuilder.DropIndex(
                name: "IX_HealthInsurances_ClinicId",
                table: "HealthInsurances");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "HealthInsurances");
        }
    }
}
