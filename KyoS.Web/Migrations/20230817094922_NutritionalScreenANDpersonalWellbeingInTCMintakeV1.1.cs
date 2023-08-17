using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class NutritionalScreenANDpersonalWellbeingInTCMintakeV11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrentPainScore",
                table: "TCMIntakePersonalWellbeing",
                newName: "Living");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Living",
                table: "TCMIntakePersonalWellbeing",
                newName: "CurrentPainScore");
        }
    }
}
