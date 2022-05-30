using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ServicePlan_LIstService3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddForeignKey(
                name: "FK_TCMDomains_TCMServicePlans_TcmServicePlanId",
                table: "TCMDomains",
                column: "TcmServicePlanId",
                principalTable: "TCMServicePlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMDomains_TCMServicePlans_TcmServicePlanId",
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
    }
}
