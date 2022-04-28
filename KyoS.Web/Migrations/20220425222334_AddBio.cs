using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddBio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FarsForm_Clients_ClientId",
                table: "FarsForm");

            migrationBuilder.CreateTable(
                name: "Bio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateBio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Setting = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CMH = table.Column<bool>(type: "bit", nullable: false),
                    Priv = table.Column<bool>(type: "bit", nullable: false),
                    BioH0031HN = table.Column<bool>(type: "bit", nullable: false),
                    BioH0031HO = table.Column<bool>(type: "bit", nullable: false),
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
                    HaveYouEverThought = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverThought_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoYouOwn = table.Column<bool>(type: "bit", nullable: false),
                    DoYouOwn_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoesClient = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverBeen = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverBeen_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasTheClient = table.Column<bool>(type: "bit", nullable: false),
                    HasTheClient_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientFamilyAbusoTrauma = table.Column<bool>(type: "bit", nullable: false),
                    DateAbuse = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PersonInvolved = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApproximateDateReport = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApproximateDateReport_Where = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelationShips = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AReferral = table.Column<bool>(type: "bit", nullable: false),
                    AReferral_Services = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AReferral_When = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AReferral_Where = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObtainRelease = table.Column<bool>(type: "bit", nullable: false),
                    WhereRecord = table.Column<bool>(type: "bit", nullable: false),
                    WhereRecord_When = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhereRecord_Where = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasTheClientVisitedPhysician = table.Column<bool>(type: "bit", nullable: false),
                    HasTheClientVisitedPhysician_Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasTheClientVisitedPhysician_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DoesTheClientExperience = table.Column<bool>(type: "bit", nullable: false),
                    DoesTheClientExperience_Where = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PleaseRatePain = table.Column<int>(type: "int", nullable: false),
                    HasClientBeenTreatedPain = table.Column<bool>(type: "bit", nullable: false),
                    HasClientBeenTreatedPain_PleaseIncludeService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasClientBeenTreatedPain_Ifnot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasClientBeenTreatedPain_Where = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObtainReleaseInformation = table.Column<bool>(type: "bit", nullable: false),
                    HasAnIllnes = table.Column<bool>(type: "bit", nullable: false),
                    EastFewer = table.Column<bool>(type: "bit", nullable: false),
                    EastFew = table.Column<bool>(type: "bit", nullable: false),
                    Has3OrMore = table.Column<bool>(type: "bit", nullable: false),
                    HasTooth = table.Column<bool>(type: "bit", nullable: false),
                    DoesNotAlways = table.Column<bool>(type: "bit", nullable: false),
                    EastAlone = table.Column<bool>(type: "bit", nullable: false),
                    Takes3OrMore = table.Column<bool>(type: "bit", nullable: false),
                    WithoutWanting = table.Column<bool>(type: "bit", nullable: false),
                    NotAlwaysPhysically = table.Column<bool>(type: "bit", nullable: false),
                    If6_ReferredTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    If6_Date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Appetite = table.Column<int>(type: "int", nullable: false),
                    Hydration = table.Column<int>(type: "int", nullable: false),
                    RecentWeight = table.Column<int>(type: "int", nullable: false),
                    SubstanceAbuse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalFamilyPsychiatric = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoesClientRequired = table.Column<bool>(type: "bit", nullable: false),
                    DoesClientRequired_Where = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObtainReleaseInformation7 = table.Column<bool>(type: "bit", nullable: false),
                    IfForeing_Born = table.Column<bool>(type: "bit", nullable: false),
                    IfForeing_AgeArrival = table.Column<int>(type: "int", nullable: false),
                    IfForeing_YearArrival = table.Column<int>(type: "int", nullable: false),
                    PrimaryLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneralDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdultCurrentExperience = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatIsTheClient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelationshipWithFamily = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Children = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IfMarried = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IfSeparated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IfSexuallyActive = table.Column<bool>(type: "bit", nullable: false),
                    PleaseProvideGoal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoYouHaveAnyReligious = table.Column<bool>(type: "bit", nullable: false),
                    WhatIsYourLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoYouHaveAnyVisual = table.Column<bool>(type: "bit", nullable: false),
                    HigHestEducation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoYouHaveAnyPhysical = table.Column<bool>(type: "bit", nullable: false),
                    CanClientFollow = table.Column<bool>(type: "bit", nullable: false),
                    ProvideIntegratedSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentNeeds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Treatmentrecomendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicensedPractitioner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSignatureLicensedPractitioner = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnlicensedTherapist = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSignatureUnlicensedTherapist = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IConcurWhitDiagnistic = table.Column<bool>(type: "bit", nullable: false),
                    AlternativeDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSignatureSupervisor = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bio_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bio_BehavioralHistoryEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BioId = table.Column<int>(type: "int", nullable: true),
                    Problem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bio_BehavioralHistoryEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bio_BehavioralHistoryEntity_Bio_BioId",
                        column: x => x.BioId,
                        principalTable: "Bio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bio_Client_FK",
                table: "Bio",
                column: "Client_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bio_BehavioralHistoryEntity_BioId",
                table: "Bio_BehavioralHistoryEntity",
                column: "BioId");

            migrationBuilder.AddForeignKey(
                name: "FK_FarsForm_Clients_ClientId",
                table: "FarsForm",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FarsForm_Clients_ClientId",
                table: "FarsForm");

            migrationBuilder.DropTable(
                name: "Bio_BehavioralHistoryEntity");

            migrationBuilder.DropTable(
                name: "Bio");

            migrationBuilder.AddForeignKey(
                name: "FK_FarsForm_Clients_ClientId",
                table: "FarsForm",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
