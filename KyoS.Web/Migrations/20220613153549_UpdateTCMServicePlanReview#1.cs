using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateTCMServicePlanReview1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviews_TCMServicePlans_TcmServicePlanId",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropIndex(
                name: "IX_TCMServicePlanReviews_Id",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropIndex(
                name: "IX_TCMServicePlanReviews_TcmServicePlanId",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "TcmServicePlanId",
                table: "TCMServicePlanReviews");

            migrationBuilder.AddColumn<int>(
                name: "TcmServicePlan_FK",
                table: "TCMServicePlanReviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviews_TcmServicePlan_FK",
                table: "TCMServicePlanReviews",
                column: "TcmServicePlan_FK",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviews_TCMServicePlans_TcmServicePlan_FK",
                table: "TCMServicePlanReviews",
                column: "TcmServicePlan_FK",
                principalTable: "TCMServicePlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviews_TCMServicePlans_TcmServicePlan_FK",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropIndex(
                name: "IX_TCMServicePlanReviews_TcmServicePlan_FK",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "TcmServicePlan_FK",
                table: "TCMServicePlanReviews");

            migrationBuilder.AddColumn<int>(
                name: "TcmServicePlanId",
                table: "TCMServicePlanReviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviews_Id",
                table: "TCMServicePlanReviews",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviews_TcmServicePlanId",
                table: "TCMServicePlanReviews",
                column: "TcmServicePlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviews_TCMServicePlans_TcmServicePlanId",
                table: "TCMServicePlanReviews",
                column: "TcmServicePlanId",
                principalTable: "TCMServicePlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
