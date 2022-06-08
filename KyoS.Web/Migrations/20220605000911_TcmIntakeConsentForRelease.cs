using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TcmIntakeConsentForRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeConsentForRelease",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToRelease = table.Column<bool>(type: "bit", nullable: false),
                    InForm_VerbalInformation = table.Column<bool>(type: "bit", nullable: false),
                    InForm_Facsimile = table.Column<bool>(type: "bit", nullable: false),
                    InForm_WrittenRecords = table.Column<bool>(type: "bit", nullable: false),
                    ForPurpose_Treatment = table.Column<bool>(type: "bit", nullable: false),
                    ForPurpose_CaseManagement = table.Column<bool>(type: "bit", nullable: false),
                    ForPurpose_Other = table.Column<bool>(type: "bit", nullable: false),
                    ForPurpose_OtherExplain = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_TCMIntakeConsentForRelease", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeConsentForRelease_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeConsentForRelease_TcmClient_FK",
                table: "TCMIntakeConsentForRelease",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeConsentForRelease");
        }
    }
}
