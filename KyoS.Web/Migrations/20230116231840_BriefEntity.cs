using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class BriefEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BriefEntityId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brief",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateBio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Setting = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CMH = table.Column<bool>(type: "bit", nullable: false),
                    Priv = table.Column<bool>(type: "bit", nullable: false),
                    BioH0031HN = table.Column<bool>(type: "bit", nullable: false),
                    IDAH0031HO = table.Column<bool>(type: "bit", nullable: false),
                    PresentingProblem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientAssessmentSituation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamilyAssessmentSituation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamilyEmotional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalAssessment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Appearance_Disheveled = table.Column<bool>(type: "bit", nullable: false),
                    Appearance_FairHygiene = table.Column<bool>(type: "bit", nullable: false),
                    Appearance_Cleaned = table.Column<bool>(type: "bit", nullable: false),
                    Appearance_WellGroomed = table.Column<bool>(type: "bit", nullable: false),
                    Appearance_Bizarre = table.Column<bool>(type: "bit", nullable: false),
                    Motor_Normal = table.Column<bool>(type: "bit", nullable: false),
                    Motor_Agitated = table.Column<bool>(type: "bit", nullable: false),
                    Motor_Retardation = table.Column<bool>(type: "bit", nullable: false),
                    Motor_RestLess = table.Column<bool>(type: "bit", nullable: false),
                    Motor_Akathisia = table.Column<bool>(type: "bit", nullable: false),
                    Motor_Tremor = table.Column<bool>(type: "bit", nullable: false),
                    Motor_Other = table.Column<bool>(type: "bit", nullable: false),
                    Speech_Normal = table.Column<bool>(type: "bit", nullable: false),
                    Speech_Loud = table.Column<bool>(type: "bit", nullable: false),
                    Speech_Mumbled = table.Column<bool>(type: "bit", nullable: false),
                    Speech_Stutters = table.Column<bool>(type: "bit", nullable: false),
                    Speech_Pressured = table.Column<bool>(type: "bit", nullable: false),
                    Speech_Rapid = table.Column<bool>(type: "bit", nullable: false),
                    Speech_Impoverished = table.Column<bool>(type: "bit", nullable: false),
                    Speech_Slow = table.Column<bool>(type: "bit", nullable: false),
                    Speech_Slurred = table.Column<bool>(type: "bit", nullable: false),
                    Speech_Other = table.Column<bool>(type: "bit", nullable: false),
                    Affect_Appropriate = table.Column<bool>(type: "bit", nullable: false),
                    Affect_labile = table.Column<bool>(type: "bit", nullable: false),
                    Affect_Flat = table.Column<bool>(type: "bit", nullable: false),
                    Affect_Tearful_Sad = table.Column<bool>(type: "bit", nullable: false),
                    Affect_Expansive = table.Column<bool>(type: "bit", nullable: false),
                    Affect_Anxious = table.Column<bool>(type: "bit", nullable: false),
                    Affect_Blunted = table.Column<bool>(type: "bit", nullable: false),
                    Affect_Angry = table.Column<bool>(type: "bit", nullable: false),
                    Affect_Constricted = table.Column<bool>(type: "bit", nullable: false),
                    Affect_Other = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_Organized = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_Obsessive = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_FightIdeas = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_Disorganized = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_Tangential = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_LooseAssociations = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_GoalDirected = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_Circumstantial = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_Other = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_Irrational = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_Preoccupied = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_Rigid = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtProcess_Blocking = table.Column<bool>(type: "bit", nullable: false),
                    Mood_Euthymic = table.Column<bool>(type: "bit", nullable: false),
                    Mood_Depressed = table.Column<bool>(type: "bit", nullable: false),
                    Mood_Anxious = table.Column<bool>(type: "bit", nullable: false),
                    Mood_Euphoric = table.Column<bool>(type: "bit", nullable: false),
                    Mood_Angry = table.Column<bool>(type: "bit", nullable: false),
                    Mood_Maniac = table.Column<bool>(type: "bit", nullable: false),
                    Mood_Other = table.Column<bool>(type: "bit", nullable: false),
                    Judgment_Good = table.Column<bool>(type: "bit", nullable: false),
                    Judgment_Fair = table.Column<bool>(type: "bit", nullable: false),
                    Judgment_Poor = table.Column<bool>(type: "bit", nullable: false),
                    Judgment_Other = table.Column<bool>(type: "bit", nullable: false),
                    Insight_Good = table.Column<bool>(type: "bit", nullable: false),
                    Insight_Fair = table.Column<bool>(type: "bit", nullable: false),
                    Insight_Poor = table.Column<bool>(type: "bit", nullable: false),
                    Insight_Other = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtContent_Relevant = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtContent_RealityBased = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtContent_Hallucinations = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtContent_Hallucinations_Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoughtContent_Delusions = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtContent_Delusions_Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Oriented_FullOriented = table.Column<bool>(type: "bit", nullable: false),
                    Lacking_Time = table.Column<bool>(type: "bit", nullable: false),
                    Lacking_Place = table.Column<bool>(type: "bit", nullable: false),
                    Lacking_Person = table.Column<bool>(type: "bit", nullable: false),
                    Lacking_Location = table.Column<bool>(type: "bit", nullable: false),
                    RiskToSelf_Low = table.Column<bool>(type: "bit", nullable: false),
                    RiskToSelf_Medium = table.Column<bool>(type: "bit", nullable: false),
                    RiskToSelf_High = table.Column<bool>(type: "bit", nullable: false),
                    RiskToSelf_Chronic = table.Column<bool>(type: "bit", nullable: false),
                    RiskToOther_Low = table.Column<bool>(type: "bit", nullable: false),
                    RiskToOther_Medium = table.Column<bool>(type: "bit", nullable: false),
                    RiskToOther_High = table.Column<bool>(type: "bit", nullable: false),
                    RiskToOther_Chronic = table.Column<bool>(type: "bit", nullable: false),
                    SafetyPlan = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientDenied = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverThought = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverThought_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoYouOwn = table.Column<bool>(type: "bit", nullable: false),
                    DoYouOwn_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoesClient = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverBeen = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverBeen_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasTheClient = table.Column<bool>(type: "bit", nullable: false),
                    HasTheClient_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SumanrOfFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Treatmentrecomendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    DateSignatureLicensedPractitioner = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentsAssistantId = table.Column<int>(type: "int", nullable: true),
                    DateSignatureUnlicensedTherapist = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IConcurWhitDiagnistic = table.Column<bool>(type: "bit", nullable: false),
                    AlternativeDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSignatureSupervisor = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brief", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brief_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Brief_DocumentsAssistant_DocumentsAssistantId",
                        column: x => x.DocumentsAssistantId,
                        principalTable: "DocumentsAssistant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Brief_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_BriefEntityId",
                table: "Messages",
                column: "BriefEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Brief_Client_FK",
                table: "Brief",
                column: "Client_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brief_DocumentsAssistantId",
                table: "Brief",
                column: "DocumentsAssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_Brief_SupervisorId",
                table: "Brief",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Brief_BriefEntityId",
                table: "Messages",
                column: "BriefEntityId",
                principalTable: "Brief",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Brief_BriefEntityId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "Brief");

            migrationBuilder.DropIndex(
                name: "IX_Messages_BriefEntityId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "BriefEntityId",
                table: "Messages");
        }
    }
}
