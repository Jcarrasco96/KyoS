using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class DischargeJoinCommission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClientMoveOutArea",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExtendedHospitalization",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Follow_up",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "JoinCommission",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MinimalProgress",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ModerateProgress",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NoProgress",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PlanCompletePartially",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Regression",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SignificantProgress",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SummaryOfPresentingProblems",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TreatmentSummary",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UnableToDetermine",
                table: "Discharge",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientMoveOutArea",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ExtendedHospitalization",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "Follow_up",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "JoinCommission",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "MinimalProgress",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "ModerateProgress",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "NoProgress",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "PlanCompletePartially",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "Regression",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "SignificantProgress",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "SummaryOfPresentingProblems",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "TreatmentSummary",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "UnableToDetermine",
                table: "Discharge");
        }
    }
}
