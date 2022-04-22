using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Intake_added_field_admissionedFor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeTuberculosis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeTransportation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeScreenings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeOrientationCheckList",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeMedicalHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeFeeAgreement",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeConsumerRights",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeConsentPhotograph",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeConsentForTreatment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeAcknowledgement",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdmissionedFor",
                table: "IntakeAccessToServices",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeTuberculosis");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeTransportation");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeScreenings");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeOrientationCheckList");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeFeeAgreement");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeConsumerRights");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeConsentPhotograph");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeConsentForTreatment");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeAcknowledgement");

            migrationBuilder.DropColumn(
                name: "AdmissionedFor",
                table: "IntakeAccessToServices");
        }
    }
}
