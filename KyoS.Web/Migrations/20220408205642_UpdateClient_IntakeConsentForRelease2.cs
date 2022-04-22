using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateClient_IntakeConsentForRelease2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForThePorpuseOf",
                table: "IntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "InTheFormOf",
                table: "IntakeConsentForRelease");

            migrationBuilder.AddColumn<bool>(
                name: "ForPurpose_CaseManagement",
                table: "IntakeConsentForRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ForPurpose_Other",
                table: "IntakeConsentForRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ForPurpose_OtherExplain",
                table: "IntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ForPurpose_Treatment",
                table: "IntakeConsentForRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InForm_Facsimile",
                table: "IntakeConsentForRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InForm_VerbalInformation",
                table: "IntakeConsentForRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InForm_WrittenRecords",
                table: "IntakeConsentForRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForPurpose_CaseManagement",
                table: "IntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "ForPurpose_Other",
                table: "IntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "ForPurpose_OtherExplain",
                table: "IntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "ForPurpose_Treatment",
                table: "IntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "InForm_Facsimile",
                table: "IntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "InForm_VerbalInformation",
                table: "IntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "InForm_WrittenRecords",
                table: "IntakeConsentForRelease");

            migrationBuilder.AddColumn<int>(
                name: "ForThePorpuseOf",
                table: "IntakeConsentForRelease",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InTheFormOf",
                table: "IntakeConsentForRelease",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
