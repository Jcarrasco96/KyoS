using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddClinic_ThemeEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Themes_Clinics_ClinicId",
                table: "Themes");

            migrationBuilder.RenameColumn(
                name: "ClinicId",
                table: "Themes",
                newName: "ClinicEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Themes_ClinicId",
                table: "Themes",
                newName: "IX_Themes_ClinicEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Themes_Clinics_ClinicEntityId",
                table: "Themes",
                column: "ClinicEntityId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Themes_Clinics_ClinicEntityId",
                table: "Themes");

            migrationBuilder.RenameColumn(
                name: "ClinicEntityId",
                table: "Themes",
                newName: "ClinicId");

            migrationBuilder.RenameIndex(
                name: "IX_Themes_ClinicEntityId",
                table: "Themes",
                newName: "IX_Themes_ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Themes_Clinics_ClinicId",
                table: "Themes",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
