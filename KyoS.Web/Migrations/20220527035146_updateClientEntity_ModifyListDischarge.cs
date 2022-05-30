using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class updateClientEntity_ModifyListDischarge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discharge_Clients_Client_FK",
                table: "Discharge");

            migrationBuilder.DropIndex(
                name: "IX_Discharge_Client_FK",
                table: "Discharge");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Discharge",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discharge_ClientId",
                table: "Discharge",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Discharge_Clients_ClientId",
                table: "Discharge",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discharge_Clients_ClientId",
                table: "Discharge");

            migrationBuilder.DropIndex(
                name: "IX_Discharge_ClientId",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Discharge");

            migrationBuilder.CreateIndex(
                name: "IX_Discharge_Client_FK",
                table: "Discharge",
                column: "Client_FK",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Discharge_Clients_Client_FK",
                table: "Discharge",
                column: "Client_FK",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
