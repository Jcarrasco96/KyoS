using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Add_FarsForm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FarsForm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    ContractorID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DcfEvaluation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EvaluationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    M_GafScore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RaterEducation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RaterFMHI = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubstanceAbusoHistory = table.Column<bool>(type: "bit", nullable: false),
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
                    ContID1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContID2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContID3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderLocal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicaidRecipientID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicaidProviderID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MCOID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignatureDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarsForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarsForm_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FarsForm_ClientId",
                table: "FarsForm",
                column: "ClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FarsForm");
        }
    }
}
