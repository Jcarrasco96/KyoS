using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ImprovesAddProviderMedicaidInCLinicEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProviderId",
                table: "Clinics",
                newName: "ProviderTaxId");

            migrationBuilder.AddColumn<string>(
                name: "ProviderMedicaidId",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderMedicaidId",
                table: "Clinics");

            migrationBuilder.RenameColumn(
                name: "ProviderTaxId",
                table: "Clinics",
                newName: "ProviderId");
        }
    }
}
