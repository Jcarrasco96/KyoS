using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMObjetive_AddResponsible1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMDomains_TcmServicePlanId",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.RenameColumn(
                name: "TcmServicePlanId",
                table: "TCMServicePlanReviewDomains",
                newName: "TcmDomainId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMServicePlanReviewDomains_TcmServicePlanId",
                table: "TCMServicePlanReviewDomains",
                newName: "IX_TCMServicePlanReviewDomains_TcmDomainId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMDomains_TcmDomainId",
                table: "TCMServicePlanReviewDomains",
                column: "TcmDomainId",
                principalTable: "TCMDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMDomains_TcmDomainId",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.RenameColumn(
                name: "TcmDomainId",
                table: "TCMServicePlanReviewDomains",
                newName: "TcmServicePlanId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMServicePlanReviewDomains_TcmDomainId",
                table: "TCMServicePlanReviewDomains",
                newName: "IX_TCMServicePlanReviewDomains_TcmServicePlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMDomains_TcmServicePlanId",
                table: "TCMServicePlanReviewDomains",
                column: "TcmServicePlanId",
                principalTable: "TCMDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
