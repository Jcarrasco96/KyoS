using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMServicePlan_update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CaseManagerId",
                table: "TCMServicePlans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlans_CaseManagerId",
                table: "TCMServicePlans",
                column: "CaseManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlans_CaseManagers_CaseManagerId",
                table: "TCMServicePlans",
                column: "CaseManagerId",
                principalTable: "CaseManagers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlans_CaseManagers_CaseManagerId",
                table: "TCMServicePlans");

            migrationBuilder.DropIndex(
                name: "IX_TCMServicePlans_CaseManagerId",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "CaseManagerId",
                table: "TCMServicePlans");
        }
    }
}
