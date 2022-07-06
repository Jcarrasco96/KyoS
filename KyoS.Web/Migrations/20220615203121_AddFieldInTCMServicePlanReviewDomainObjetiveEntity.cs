using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddFieldInTCMServicePlanReviewDomainObjetiveEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviewDomainObjectives_TCMServicePlanReviewDomains_TCMServicePlanReviewDomainEntityId",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.RenameColumn(
                name: "TCMServicePlanReviewDomainEntityId",
                table: "TCMServicePlanReviewDomainObjectives",
                newName: "tcmServicePlanReviewDomainId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMServicePlanReviewDomainObjectives_TCMServicePlanReviewDomainEntityId",
                table: "TCMServicePlanReviewDomainObjectives",
                newName: "IX_TCMServicePlanReviewDomainObjectives_tcmServicePlanReviewDomainId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviewDomainObjectives_TCMServicePlanReviewDomains_tcmServicePlanReviewDomainId",
                table: "TCMServicePlanReviewDomainObjectives",
                column: "tcmServicePlanReviewDomainId",
                principalTable: "TCMServicePlanReviewDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviewDomainObjectives_TCMServicePlanReviewDomains_tcmServicePlanReviewDomainId",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.RenameColumn(
                name: "tcmServicePlanReviewDomainId",
                table: "TCMServicePlanReviewDomainObjectives",
                newName: "TCMServicePlanReviewDomainEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMServicePlanReviewDomainObjectives_tcmServicePlanReviewDomainId",
                table: "TCMServicePlanReviewDomainObjectives",
                newName: "IX_TCMServicePlanReviewDomainObjectives_TCMServicePlanReviewDomainEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviewDomainObjectives_TCMServicePlanReviewDomains_TCMServicePlanReviewDomainEntityId",
                table: "TCMServicePlanReviewDomainObjectives",
                column: "TCMServicePlanReviewDomainEntityId",
                principalTable: "TCMServicePlanReviewDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
