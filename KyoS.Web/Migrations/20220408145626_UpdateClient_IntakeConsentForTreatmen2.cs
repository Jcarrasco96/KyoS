using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateClient_IntakeConsentForTreatmen2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Editing",
                table: "IntakeConsentForTreatment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Editing",
                table: "IntakeConsentForTreatment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
