using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ImprovesTCMIntakeForRelease2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Discaherge",
                table: "TCMIntakeConsentForRelease",
                newName: "Discharge");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "TCMIntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityStateZip",
                table: "TCMIntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaxNo",
                table: "TCMIntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameOfFacility",
                table: "TCMIntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNo",
                table: "TCMIntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "CityStateZip",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "FaxNo",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "NameOfFacility",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "PhoneNo",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.RenameColumn(
                name: "Discharge",
                table: "TCMIntakeConsentForRelease",
                newName: "Discaherge");
        }
    }
}
