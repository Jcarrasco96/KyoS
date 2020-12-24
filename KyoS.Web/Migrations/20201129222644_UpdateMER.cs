using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateMER : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Themes_Clinics_ClinicEntityId",
                table: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_Themes_ClinicEntityId",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "ClinicEntityId",
                table: "Themes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicEntityId",
                table: "Themes",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Themes_ClinicEntityId",
                table: "Themes",
                column: "ClinicEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Themes_Clinics_ClinicEntityId",
                table: "Themes",
                column: "ClinicEntityId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
