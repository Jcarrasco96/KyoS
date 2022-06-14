using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateDischargeCRUDNewModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BriefHistory",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ConditionalDischarge",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "CourseTreatment",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "FollowDischarge",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ReasonDischarge",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ReferralAgency1",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ReferralAgency2",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ReferralContactPersonal1",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ReferralContactPersonal2",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ReferralFor1",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ReferralFor2",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ReferralHoursOperation1",
                table: "Discharge");

            migrationBuilder.RenameColumn(
                name: "TreatmentPlanObjCumpl",
                table: "Discharge",
                newName: "Termination");

            migrationBuilder.RenameColumn(
                name: "ReferralPhone2",
                table: "Discharge",
                newName: "ReferralsTo");

            migrationBuilder.RenameColumn(
                name: "ReferralPhone1",
                table: "Discharge",
                newName: "Other_Explain");

            migrationBuilder.RenameColumn(
                name: "ReferralHoursOperation2",
                table: "Discharge",
                newName: "DischargeDiagnosis");

            migrationBuilder.RenameColumn(
                name: "PhysicallyUnstable",
                table: "Discharge",
                newName: "ProgramPSR");

            migrationBuilder.RenameColumn(
                name: "Others",
                table: "Discharge",
                newName: "ProgramInd");

            migrationBuilder.RenameColumn(
                name: "Hospitalization",
                table: "Discharge",
                newName: "ProgramGroup");

            migrationBuilder.RenameColumn(
                name: "ClientReferred",
                table: "Discharge",
                newName: "ProgramClubHouse");

            migrationBuilder.RenameColumn(
                name: "ClientMoved",
                table: "Discharge",
                newName: "PrognosisPoor");

            migrationBuilder.RenameColumn(
                name: "ClientDischargeAgainst",
                table: "Discharge",
                newName: "PrognosisGuarded");

            migrationBuilder.RenameColumn(
                name: "ClientDeceased",
                table: "Discharge",
                newName: "PrognosisGood");

            migrationBuilder.RenameColumn(
                name: "AgencyDischargeClient",
                table: "Discharge",
                newName: "PrognosisFair");

            migrationBuilder.AddColumn<bool>(
                name: "Administrative",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClientTransferred",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClinicalCoherente",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClinicalInRemission",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClinicalIncoherente",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClinicalStable",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClinicalUnpredictable",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClinicalUnstable",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CompletedTreatment",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LeftBefore",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NonCompliant",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Other",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Administrative",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ClientTransferred",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ClinicalCoherente",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ClinicalInRemission",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ClinicalIncoherente",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ClinicalStable",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ClinicalUnpredictable",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ClinicalUnstable",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "CompletedTreatment",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "LeftBefore",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "NonCompliant",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "Other",
                table: "Discharge");

            migrationBuilder.RenameColumn(
                name: "Termination",
                table: "Discharge",
                newName: "TreatmentPlanObjCumpl");

            migrationBuilder.RenameColumn(
                name: "ReferralsTo",
                table: "Discharge",
                newName: "ReferralPhone2");

            migrationBuilder.RenameColumn(
                name: "ProgramPSR",
                table: "Discharge",
                newName: "PhysicallyUnstable");

            migrationBuilder.RenameColumn(
                name: "ProgramInd",
                table: "Discharge",
                newName: "Others");

            migrationBuilder.RenameColumn(
                name: "ProgramGroup",
                table: "Discharge",
                newName: "Hospitalization");

            migrationBuilder.RenameColumn(
                name: "ProgramClubHouse",
                table: "Discharge",
                newName: "ClientReferred");

            migrationBuilder.RenameColumn(
                name: "PrognosisPoor",
                table: "Discharge",
                newName: "ClientMoved");

            migrationBuilder.RenameColumn(
                name: "PrognosisGuarded",
                table: "Discharge",
                newName: "ClientDischargeAgainst");

            migrationBuilder.RenameColumn(
                name: "PrognosisGood",
                table: "Discharge",
                newName: "ClientDeceased");

            migrationBuilder.RenameColumn(
                name: "PrognosisFair",
                table: "Discharge",
                newName: "AgencyDischargeClient");

            migrationBuilder.RenameColumn(
                name: "Other_Explain",
                table: "Discharge",
                newName: "ReferralPhone1");

            migrationBuilder.RenameColumn(
                name: "DischargeDiagnosis",
                table: "Discharge",
                newName: "ReferralHoursOperation2");

            migrationBuilder.AddColumn<string>(
                name: "BriefHistory",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConditionalDischarge",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CourseTreatment",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FollowDischarge",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReasonDischarge",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralAgency1",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralAgency2",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralContactPersonal1",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralContactPersonal2",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralFor1",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralFor2",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralHoursOperation1",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
