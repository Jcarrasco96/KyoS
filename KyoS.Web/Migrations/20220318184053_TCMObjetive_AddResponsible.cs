using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMObjetive_AddResponsible : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMServicePlans_TcmServicePlanId",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "Adendum",
                table: "TCMObjetives");

            migrationBuilder.AddColumn<string>(
                name: "Responsible",
                table: "TCMObjetives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMDomains_TcmServicePlanId",
                table: "TCMServicePlanReviewDomains",
                column: "TcmServicePlanId",
                principalTable: "TCMDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMDomains_TcmServicePlanId",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "Responsible",
                table: "TCMObjetives");

            migrationBuilder.AddColumn<int>(
                name: "Adendum",
                table: "TCMObjetives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlanReviewDomains_TCMServicePlans_TcmServicePlanId",
                table: "TCMServicePlanReviewDomains",
                column: "TcmServicePlanId",
                principalTable: "TCMServicePlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
