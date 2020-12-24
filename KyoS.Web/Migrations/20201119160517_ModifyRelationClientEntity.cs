using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyRelationClientEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Facilitators_FacilitatorId",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "FacilitatorId",
                table: "Clients",
                newName: "ClinicId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_FacilitatorId",
                table: "Clients",
                newName: "IX_Clients_ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Clinics_ClinicId",
                table: "Clients",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Clinics_ClinicId",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "ClinicId",
                table: "Clients",
                newName: "FacilitatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_ClinicId",
                table: "Clients",
                newName: "IX_Clients_FacilitatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Facilitators_FacilitatorId",
                table: "Clients",
                column: "FacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
