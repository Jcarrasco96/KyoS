using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Update_Medication1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationEntity_Clients_ClientId",
                table: "MedicationEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicationEntity",
                table: "MedicationEntity");

            migrationBuilder.RenameTable(
                name: "MedicationEntity",
                newName: "Medication");

            migrationBuilder.RenameIndex(
                name: "IX_MedicationEntity_ClientId",
                table: "Medication",
                newName: "IX_Medication_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Medication",
                table: "Medication",
                column: "Id");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medication_Clients_ClientId",
                table: "Medication");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Medication",
                table: "Medication");

            migrationBuilder.DropIndex(
                name: "IX_Medication_Id",
                table: "Medication");

            migrationBuilder.RenameTable(
                name: "Medication",
                newName: "MedicationEntity");

            migrationBuilder.RenameIndex(
                name: "IX_Medication_ClientId",
                table: "MedicationEntity",
                newName: "IX_MedicationEntity_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicationEntity",
                table: "MedicationEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationEntity_Clients_ClientId",
                table: "MedicationEntity",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
