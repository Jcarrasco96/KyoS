using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMServicePlan_update8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlans_Clinics_ClinicId",
                table: "TCMServicePlans");

            migrationBuilder.DropIndex(
                name: "IX_TCMServicePlans_ClinicId",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "TCMServicePlans");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "TCMServicePlans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlans_ClinicId",
                table: "TCMServicePlans",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlans_Clinics_ClinicId",
                table: "TCMServicePlans",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
