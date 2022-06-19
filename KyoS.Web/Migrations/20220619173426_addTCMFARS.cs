using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMFARS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RaterEducation",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RaterFMHCertification",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TCMFarsFormEntityId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TCMFarsForm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCMClientId = table.Column<int>(type: "int", nullable: true),
                    ContractorID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DcfEvaluation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EvaluationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    M_GafScore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RaterEducation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RaterFMHI = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubstanceAbusoHistory = table.Column<int>(type: "int", nullable: false),
                    DepressionScale = table.Column<int>(type: "int", nullable: false),
                    AnxietyScale = table.Column<int>(type: "int", nullable: false),
                    HyperAffectScale = table.Column<int>(type: "int", nullable: false),
                    ThoughtProcessScale = table.Column<int>(type: "int", nullable: false),
                    CognitiveScale = table.Column<int>(type: "int", nullable: false),
                    MedicalScale = table.Column<int>(type: "int", nullable: false),
                    TraumaticsScale = table.Column<int>(type: "int", nullable: false),
                    SubstanceScale = table.Column<int>(type: "int", nullable: false),
                    InterpersonalScale = table.Column<int>(type: "int", nullable: false),
                    FamilyRelationShipsScale = table.Column<int>(type: "int", nullable: false),
                    FamilyEnvironmentScale = table.Column<int>(type: "int", nullable: false),
                    SocialScale = table.Column<int>(type: "int", nullable: false),
                    WorkScale = table.Column<int>(type: "int", nullable: false),
                    ActivitiesScale = table.Column<int>(type: "int", nullable: false),
                    AbilityScale = table.Column<int>(type: "int", nullable: false),
                    DangerToSelfScale = table.Column<int>(type: "int", nullable: false),
                    DangerToOtherScale = table.Column<int>(type: "int", nullable: false),
                    SecurityScale = table.Column<int>(type: "int", nullable: false),
                    ContID1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContID2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContID3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderLocal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicaidRecipientID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicaidProviderID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MCOID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignatureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgramEvaluation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TCMSupervisorId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMFarsForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMFarsForm_TCMClient_TCMClientId",
                        column: x => x.TCMClientId,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMFarsForm_TCMSupervisors_TCMSupervisorId",
                        column: x => x.TCMSupervisorId,
                        principalTable: "TCMSupervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_TCMFarsFormEntityId",
                table: "Messages",
                column: "TCMFarsFormEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMFarsForm_TCMClientId",
                table: "TCMFarsForm",
                column: "TCMClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMFarsForm_TCMSupervisorId",
                table: "TCMFarsForm",
                column: "TCMSupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_TCMFarsForm_TCMFarsFormEntityId",
                table: "Messages",
                column: "TCMFarsFormEntityId",
                principalTable: "TCMFarsForm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_TCMFarsForm_TCMFarsFormEntityId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "TCMFarsForm");

            migrationBuilder.DropIndex(
                name: "IX_Messages_TCMFarsFormEntityId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "RaterEducation",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "RaterFMHCertification",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "TCMFarsFormEntityId",
                table: "Messages");
        }
    }
}
