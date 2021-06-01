using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyLegalGuardianEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelationShip",
                table: "LegalGuardians");

            migrationBuilder.DropColumn(
                name: "RelationShip",
                table: "EmergencyContacts");

            migrationBuilder.AddColumn<int>(
                name: "RelationShipOfLegalGuardian",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelationShipOfLegalGuardian",
                table: "Clients");

            migrationBuilder.AddColumn<int>(
                name: "RelationShip",
                table: "LegalGuardians",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RelationShip",
                table: "EmergencyContacts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
