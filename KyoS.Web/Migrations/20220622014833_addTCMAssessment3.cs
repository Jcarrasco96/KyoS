using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMAssessment3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AbuseViolence",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Allergy",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AllergySpecify",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AreAllImmunization",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AreAllImmunizationExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AreYouPhysician",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AreYouPhysicianSpecify",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateMostRecent",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescribeAnyOther",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescribeAnyRisk",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DoesAggressiveness",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesAnxiety",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesDelusions",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesDepression",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesFearfulness",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesHallucinations",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesHelplessness",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesHopelessness",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesHyperactivity",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesImpulsivity",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesIrritability",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesLoss",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesLow",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesMood",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesNegative",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesNervousness",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesObsessive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesPanic",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesParanoia",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesPoor",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesSadness",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesSelfNeglect",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesSheUnderstand",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesSleep",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesTheClientFeel",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesWithdrawal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasClientUndergone",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasDifficultySeeingLevel",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasDifficultySeeingObjetive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasNoImpairment",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasNoUsefull",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HaveYouEverBeenToAny",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HaveYouEverUsedAlcohol",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HearingDifficulty",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HearingImpairment",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HearingNotDetermined",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Hears",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Homicidal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HowActive",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HowManyTimes",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClientPregnancy",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsClientPregnancyNA",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSheReceiving",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Issues",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MentalHealth",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NoHearing",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NoUseful",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Suicidal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VisionImpairment",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VisionNotDetermined",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WhenWas",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TCMAssessmentDrug",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmAssessmentId = table.Column<int>(type: "int", nullable: true),
                    SustanceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateBegin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastTimeUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMAssessmentDrug", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMAssessmentDrug_TCMAssessment_TcmAssessmentId",
                        column: x => x.TcmAssessmentId,
                        principalTable: "TCMAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCMAssessmentHospital",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmAssessmentId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMAssessmentHospital", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMAssessmentHospital_TCMAssessment_TcmAssessmentId",
                        column: x => x.TcmAssessmentId,
                        principalTable: "TCMAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCMAssessmentMedicalProblem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmAssessmentId = table.Column<int>(type: "int", nullable: true),
                    MedicalProblem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Client = table.Column<bool>(type: "bit", nullable: false),
                    Family = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMAssessmentMedicalProblem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMAssessmentMedicalProblem_TCMAssessment_TcmAssessmentId",
                        column: x => x.TcmAssessmentId,
                        principalTable: "TCMAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCMAssessmentSurgery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmAssessmentId = table.Column<int>(type: "int", nullable: true),
                    TypeSurgery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hospital = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMAssessmentSurgery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMAssessmentSurgery_TCMAssessment_TcmAssessmentId",
                        column: x => x.TcmAssessmentId,
                        principalTable: "TCMAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMAssessmentDrug_TcmAssessmentId",
                table: "TCMAssessmentDrug",
                column: "TcmAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMAssessmentHospital_TcmAssessmentId",
                table: "TCMAssessmentHospital",
                column: "TcmAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMAssessmentMedicalProblem_TcmAssessmentId",
                table: "TCMAssessmentMedicalProblem",
                column: "TcmAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMAssessmentSurgery_TcmAssessmentId",
                table: "TCMAssessmentSurgery",
                column: "TcmAssessmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMAssessmentDrug");

            migrationBuilder.DropTable(
                name: "TCMAssessmentHospital");

            migrationBuilder.DropTable(
                name: "TCMAssessmentMedicalProblem");

            migrationBuilder.DropTable(
                name: "TCMAssessmentSurgery");

            migrationBuilder.DropColumn(
                name: "AbuseViolence",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Allergy",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AllergySpecify",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AreAllImmunization",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AreAllImmunizationExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AreYouPhysician",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AreYouPhysicianSpecify",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DateMostRecent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DescribeAnyOther",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DescribeAnyRisk",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesAggressiveness",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesAnxiety",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesDelusions",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesDepression",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesFearfulness",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesHallucinations",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesHelplessness",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesHopelessness",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesHyperactivity",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesImpulsivity",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesIrritability",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesLoss",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesLow",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesMood",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesNegative",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesNervousness",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesObsessive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesPanic",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesParanoia",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesPoor",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesSadness",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesSelfNeglect",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesSheUnderstand",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesSleep",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesTheClientFeel",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesWithdrawal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HasClientUndergone",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HasDifficultySeeingLevel",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HasDifficultySeeingObjetive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HasNoImpairment",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HasNoUsefull",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HaveYouEverBeenToAny",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HaveYouEverUsedAlcohol",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HearingDifficulty",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HearingImpairment",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HearingNotDetermined",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Hears",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Homicidal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HowActive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HowManyTimes",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsClientPregnancy",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsClientPregnancyNA",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsSheReceiving",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Issues",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "MentalHealth",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NoHearing",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NoUseful",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Suicidal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "VisionImpairment",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "VisionNotDetermined",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhenWas",
                table: "TCMAssessment");
        }
    }
}
