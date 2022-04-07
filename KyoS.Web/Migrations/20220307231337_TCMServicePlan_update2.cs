using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMServicePlan_update2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlans_Clients_ClientId",
                table: "TCMServicePlans");

            migrationBuilder.DropIndex(
                name: "IX_TCMServicePlans_CaseNumber",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "CaseNumber",
                table: "TCMServicePlans");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "TCMServicePlans",
                newName: "TcmClientId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMServicePlans_ClientId",
                table: "TCMServicePlans",
                newName: "IX_TCMServicePlans_TcmClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlans_Id",
                table: "TCMServicePlans",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlans_Clients_TcmClientId",
                table: "TCMServicePlans",
                column: "TcmClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlans_Clients_TcmClientId",
                table: "TCMServicePlans");

            migrationBuilder.DropIndex(
                name: "IX_TCMServicePlans_Id",
                table: "TCMServicePlans");

            migrationBuilder.RenameColumn(
                name: "TcmClientId",
                table: "TCMServicePlans",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMServicePlans_TcmClientId",
                table: "TCMServicePlans",
                newName: "IX_TCMServicePlans_ClientId");

            migrationBuilder.AddColumn<int>(
                name: "CaseNumber",
                table: "TCMServicePlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlans_CaseNumber",
                table: "TCMServicePlans",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlans_Clients_ClientId",
                table: "TCMServicePlans",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
