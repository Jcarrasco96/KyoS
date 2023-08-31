using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ConsentTYPE : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsentType",
                table: "TCMIntakeConsentForRelease",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OtherAutorizedInformation",
                table: "TCMIntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherPurposeRequest",
                table: "TCMIntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentType",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "OtherAutorizedInformation",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "OtherPurposeRequest",
                table: "TCMIntakeConsentForRelease");
        }
    }
}
