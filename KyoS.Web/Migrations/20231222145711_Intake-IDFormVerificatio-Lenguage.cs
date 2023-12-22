using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class IntakeIDFormVerificatioLenguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeClientDocumentVerification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    Id_DriverLicense = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Social = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicaidId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicareCard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HealthPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Passport_Resident = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Other_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Other_Identification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSignatureLegalGuardianOrClient = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeClientDocumentVerification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeClientDocumentVerification_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntakeForeignLanguage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documents = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeForeignLanguage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeForeignLanguage_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeClientDocumentVerification_Client_FK",
                table: "IntakeClientDocumentVerification",
                column: "Client_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntakeForeignLanguage_Client_FK",
                table: "IntakeForeignLanguage",
                column: "Client_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeClientDocumentVerification");

            migrationBuilder.DropTable(
                name: "IntakeForeignLanguage");
        }
    }
}
