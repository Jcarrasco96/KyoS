using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class SignatureANDdocumentVerificationInTCMintake : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeClientDocumentVerification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    Id_DriverLicense = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Social = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicaidId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicareCard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HealthPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Passport_Resident = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Other_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Other_Identification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeClientDocumentVerification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeClientDocumentVerification_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCMIntakeClientSignatureVerification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeClientSignatureVerification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeClientSignatureVerification_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeClientDocumentVerification_TcmClient_FK",
                table: "TCMIntakeClientDocumentVerification",
                column: "TcmClient_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeClientSignatureVerification_TcmClient_FK",
                table: "TCMIntakeClientSignatureVerification",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeClientDocumentVerification");

            migrationBuilder.DropTable(
                name: "TCMIntakeClientSignatureVerification");
        }
    }
}
