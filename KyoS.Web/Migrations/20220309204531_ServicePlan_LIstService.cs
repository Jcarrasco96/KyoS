using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ServicePlan_LIstService : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TCMServicePlanEntityId",
                table: "TCMServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Finish",
                table: "TCMObjetives",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServices_TCMServicePlanEntityId",
                table: "TCMServices",
                column: "TCMServicePlanEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServices_TCMServicePlans_TCMServicePlanEntityId",
                table: "TCMServices",
                column: "TCMServicePlanEntityId",
                principalTable: "TCMServicePlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServices_TCMServicePlans_TCMServicePlanEntityId",
                table: "TCMServices");

            migrationBuilder.DropIndex(
                name: "IX_TCMServices_TCMServicePlanEntityId",
                table: "TCMServices");

            migrationBuilder.DropColumn(
                name: "TCMServicePlanEntityId",
                table: "TCMServices");

            migrationBuilder.DropColumn(
                name: "Finish",
                table: "TCMObjetives");
        }
    }
}
