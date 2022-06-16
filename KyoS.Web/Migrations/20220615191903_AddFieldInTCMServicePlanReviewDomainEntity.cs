using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddFieldInTCMServicePlanReviewDomainEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMServicePlanReviews_TCMServicePlanReviewEntityId",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.RenameColumn(
                name: "TCMServicePlanReviewEntityId",
                table: "TCMServicePlanReviewDomains",
                newName: "TcmServicePlanReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMServicePlanReviewDomains_TCMServicePlanReviewEntityId",
                table: "TCMServicePlanReviewDomains",
                newName: "IX_TCMServicePlanReviewDomains_TcmServicePlanReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMServicePlanReviews_TcmServicePlanReviewId",
                table: "TCMServicePlanReviewDomains",
                column: "TcmServicePlanReviewId",
                principalTable: "TCMServicePlanReviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMServicePlanReviews_TcmServicePlanReviewId",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.RenameColumn(
                name: "TcmServicePlanReviewId",
                table: "TCMServicePlanReviewDomains",
                newName: "TCMServicePlanReviewEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMServicePlanReviewDomains_TcmServicePlanReviewId",
                table: "TCMServicePlanReviewDomains",
                newName: "IX_TCMServicePlanReviewDomains_TCMServicePlanReviewEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMServicePlanReviews_TCMServicePlanReviewEntityId",
                table: "TCMServicePlanReviewDomains",
                column: "TCMServicePlanReviewEntityId",
                principalTable: "TCMServicePlanReviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
