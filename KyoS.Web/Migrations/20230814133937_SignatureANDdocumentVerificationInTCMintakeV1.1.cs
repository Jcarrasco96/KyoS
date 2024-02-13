using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class SignatureANDdocumentVerificationInTCMintakeV11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "TCMIntakeClientSignatureVerification",
                newName: "DateSignatureLegalGuardianOrClient");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "TCMIntakeClientDocumentVerification",
                newName: "DateSignatureLegalGuardianOrClient");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSignatureEmployee",
                table: "TCMIntakeClientSignatureVerification",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSignatureEmployee",
                table: "TCMIntakeClientDocumentVerification",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateSignatureEmployee",
                table: "TCMIntakeClientSignatureVerification");

            migrationBuilder.DropColumn(
                name: "DateSignatureEmployee",
                table: "TCMIntakeClientDocumentVerification");

            migrationBuilder.RenameColumn(
                name: "DateSignatureLegalGuardianOrClient",
                table: "TCMIntakeClientSignatureVerification",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "DateSignatureLegalGuardianOrClient",
                table: "TCMIntakeClientDocumentVerification",
                newName: "Date");
        }
    }
}
