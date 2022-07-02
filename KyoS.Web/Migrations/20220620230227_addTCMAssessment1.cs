using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMAssessment1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClientCurrently",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LegalDecisionAdLitem",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalDecisionAddress",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LegalDecisionAttomey",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalDecisionCityStateZip",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LegalDecisionLegal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalDecisionName",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LegalDecisionNone",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LegalDecisionOther",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalDecisionOtherExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LegalDecisionParent",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalDecisionPhone",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NeedOfSpecial",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NeedOfSpecialSpecify",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TypeOfAssessmentAnnual",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TypeOfAssessmentInitial",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TypeOfAssessmentOther",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfAssessmentOtherExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TypeOfAssessmentSignificant",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClientCurrently",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionAdLitem",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionAddress",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionAttomey",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionCityStateZip",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionLegal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionName",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionNone",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionOther",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionOtherExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionParent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LegalDecisionPhone",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NeedOfSpecial",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NeedOfSpecialSpecify",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TypeOfAssessmentAnnual",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TypeOfAssessmentInitial",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TypeOfAssessmentOther",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TypeOfAssessmentOtherExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TypeOfAssessmentSignificant",
                table: "TCMAssessment");
        }
    }
}
