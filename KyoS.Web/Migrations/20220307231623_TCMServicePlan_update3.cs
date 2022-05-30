using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMServicePlan_update3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlans_Clients_TcmClientId",
                table: "TCMServicePlans");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlans_TCMClient_TcmClientId",
                table: "TCMServicePlans",
                column: "TcmClientId",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlans_TCMClient_TcmClientId",
                table: "TCMServicePlans");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlans_Clients_TcmClientId",
                table: "TCMServicePlans",
                column: "TcmClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
