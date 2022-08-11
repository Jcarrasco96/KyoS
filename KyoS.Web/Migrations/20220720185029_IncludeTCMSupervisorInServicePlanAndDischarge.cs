using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class IncludeTCMSupervisorInServicePlanAndDischarge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TCMSupervisorId",
                table: "TCMServicePlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TCMSupervisorId",
                table: "TCMServicePlanReviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TCMSupervisorId",
                table: "TCMDischarge",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlans_TCMSupervisorId",
                table: "TCMServicePlans",
                column: "TCMSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlanReviews_TCMSupervisorId",
                table: "TCMServicePlanReviews",
                column: "TCMSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMDischarge_TCMSupervisorId",
                table: "TCMDischarge",
                column: "TCMSupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMDischarge_TCMSupervisors_TCMSupervisorId",
                table: "TCMDischarge",
                column: "TCMSupervisorId",
                principalTable: "TCMSupervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviews_TCMSupervisors_TCMSupervisorId",
                table: "TCMServicePlanReviews",
                column: "TCMSupervisorId",
                principalTable: "TCMSupervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlans_TCMSupervisors_TCMSupervisorId",
                table: "TCMServicePlans",
                column: "TCMSupervisorId",
                principalTable: "TCMSupervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMDischarge_TCMSupervisors_TCMSupervisorId",
                table: "TCMDischarge");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviews_TCMSupervisors_TCMSupervisorId",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlans_TCMSupervisors_TCMSupervisorId",
                table: "TCMServicePlans");

            migrationBuilder.DropIndex(
                name: "IX_TCMServicePlans_TCMSupervisorId",
                table: "TCMServicePlans");

            migrationBuilder.DropIndex(
                name: "IX_TCMServicePlanReviews_TCMSupervisorId",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropIndex(
                name: "IX_TCMDischarge_TCMSupervisorId",
                table: "TCMDischarge");

            migrationBuilder.DropColumn(
                name: "TCMSupervisorId",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "TCMSupervisorId",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "TCMSupervisorId",
                table: "TCMDischarge");
        }
    }
}
