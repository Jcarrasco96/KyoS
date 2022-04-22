using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateClient_IntakeConsentForRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeConsentForRelease",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToRelease = table.Column<bool>(type: "bit", nullable: false),
                    AuthorizeStaff = table.Column<bool>(type: "bit", nullable: false),
                    InTheFormOf = table.Column<int>(type: "int", nullable: false),
                    ForThePorpuseOf = table.Column<int>(type: "int", nullable: false),
                    Discaherge = table.Column<bool>(type: "bit", nullable: false),
                    SchoolRecord = table.Column<bool>(type: "bit", nullable: false),
                    ProgressReports = table.Column<bool>(type: "bit", nullable: false),
                    IncidentReport = table.Column<bool>(type: "bit", nullable: false),
                    PsychologycalEvaluation = table.Column<bool>(type: "bit", nullable: false),
                    History = table.Column<bool>(type: "bit", nullable: false),
                    LabWork = table.Column<bool>(type: "bit", nullable: false),
                    HospitalRecord = table.Column<bool>(type: "bit", nullable: false),
                    Other = table.Column<bool>(type: "bit", nullable: false),
                    Other_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documents = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeConsentForRelease", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeConsentForRelease_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeConsentForRelease_Client_FK",
                table: "IntakeConsentForRelease",
                column: "Client_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeConsentForRelease");
        }
    }
}
