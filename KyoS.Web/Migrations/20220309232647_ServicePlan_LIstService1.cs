using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ServicePlan_LIstService1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMDomains_Clinics_ClinicId",
                table: "TCMDomains");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMDomains_TCMServicePlans_TcmServicePlanId",
                table: "TCMDomains");

            migrationBuilder.DropIndex(
                name: "IX_TCMDomains_ClinicId",
                table: "TCMDomains");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "TCMDomains");

            migrationBuilder.RenameColumn(
                name: "TcmServicePlanId",
                table: "TCMDomains",
                newName: "TCMServicePlanEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMDomains_TcmServicePlanId",
                table: "TCMDomains",
                newName: "IX_TCMDomains_TCMServicePlanEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMDomains_TCMServicePlans_TCMServicePlanEntityId",
                table: "TCMDomains",
                column: "TCMServicePlanEntityId",
                principalTable: "TCMServicePlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMDomains_TCMServicePlans_TCMServicePlanEntityId",
                table: "TCMDomains");

            migrationBuilder.RenameColumn(
                name: "TCMServicePlanEntityId",
                table: "TCMDomains",
                newName: "TcmServicePlanId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMDomains_TCMServicePlanEntityId",
                table: "TCMDomains",
                newName: "IX_TCMDomains_TcmServicePlanId");

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "TCMDomains",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMDomains_ClinicId",
                table: "TCMDomains",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMDomains_Clinics_ClinicId",
                table: "TCMDomains",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMDomains_TCMServicePlans_TcmServicePlanId",
                table: "TCMDomains",
                column: "TcmServicePlanId",
                principalTable: "TCMServicePlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
