using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Update_Medication4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medication_Clients_ClientId",
                table: "Medication");

            migrationBuilder.DropIndex(
                name: "IX_Medication_Id",
                table: "Medication");

            migrationBuilder.AddForeignKey(
                name: "FK_Medication_Clients_ClientId",
                table: "Medication",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medication_Clients_ClientId",
                table: "Medication");

            migrationBuilder.CreateIndex(
                name: "IX_Medication_Id",
                table: "Medication",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Medication_Clients_ClientId",
                table: "Medication",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
