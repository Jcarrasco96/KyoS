using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class updateMTP_fix_Model_FSHS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalRecommended",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AdmissionDateMTP",
                table: "MTPs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ClientLimitation",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientStrengths",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfUpdate",
                table: "MTPs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Family",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FamilyCode",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FamilyDuration",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FamilyFrecuency",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FamilyUnits",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Group",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GroupCode",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupDuration",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GroupFrecuency",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupUnits",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Health",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HealthWhere",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Individual",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IndividualCode",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IndividualDuration",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IndividualFrecuency",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IndividualUnits",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Legal",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalWhere",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Medication",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MedicationCode",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedicationDuration",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MedicationFrecuency",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedicationUnits",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Other",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OtherWhere",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Paint",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaintWhere",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Psychosocial",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PsychosocialCode",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PsychosocialDuration",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PsychosocialFrecuency",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PsychosocialUnits",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RationaleForUpdate",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Substance",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubstanceWhere",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalRecommended",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "AdmissionDateMTP",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "ClientLimitation",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "ClientStrengths",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "DateOfUpdate",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Family",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "FamilyCode",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "FamilyDuration",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "FamilyFrecuency",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "FamilyUnits",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "GroupCode",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "GroupDuration",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "GroupFrecuency",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "GroupUnits",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Health",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "HealthWhere",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Individual",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "IndividualCode",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "IndividualDuration",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "IndividualFrecuency",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "IndividualUnits",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Legal",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "LegalWhere",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Medication",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "MedicationCode",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "MedicationDuration",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "MedicationFrecuency",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "MedicationUnits",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Other",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "OtherWhere",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Paint",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "PaintWhere",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Psychosocial",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "PsychosocialCode",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "PsychosocialDuration",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "PsychosocialFrecuency",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "PsychosocialUnits",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "RationaleForUpdate",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Substance",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "SubstanceWhere",
                table: "MTPs");
        }
    }
}
