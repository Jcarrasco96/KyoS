using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddEntityNotePs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotesP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Workday_Client_FK = table.Column<int>(type: "int", nullable: false),
                    Workday_CientId = table.Column<int>(type: "int", nullable: true),
                    PlanNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateOfApprove = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attentive = table.Column<bool>(type: "bit", nullable: false),
                    Depressed = table.Column<bool>(type: "bit", nullable: false),
                    Inattentive = table.Column<bool>(type: "bit", nullable: false),
                    Angry = table.Column<bool>(type: "bit", nullable: false),
                    Sad = table.Column<bool>(type: "bit", nullable: false),
                    FlatAffect = table.Column<bool>(type: "bit", nullable: false),
                    Anxious = table.Column<bool>(type: "bit", nullable: false),
                    PositiveEffect = table.Column<bool>(type: "bit", nullable: false),
                    Oriented3x = table.Column<bool>(type: "bit", nullable: false),
                    Oriented2x = table.Column<bool>(type: "bit", nullable: false),
                    Oriented1x = table.Column<bool>(type: "bit", nullable: false),
                    Impulsive = table.Column<bool>(type: "bit", nullable: false),
                    Labile = table.Column<bool>(type: "bit", nullable: false),
                    Withdrawn = table.Column<bool>(type: "bit", nullable: false),
                    RelatesWell = table.Column<bool>(type: "bit", nullable: false),
                    DecreasedEyeContact = table.Column<bool>(type: "bit", nullable: false),
                    AppropiateEyeContact = table.Column<bool>(type: "bit", nullable: false),
                    Minimal = table.Column<bool>(type: "bit", nullable: false),
                    Slow = table.Column<bool>(type: "bit", nullable: false),
                    Steady = table.Column<bool>(type: "bit", nullable: false),
                    GoodExcelent = table.Column<bool>(type: "bit", nullable: false),
                    IncreasedDifficultiesNoted = table.Column<bool>(type: "bit", nullable: false),
                    Complicated = table.Column<bool>(type: "bit", nullable: false),
                    DevelopingInsight = table.Column<bool>(type: "bit", nullable: false),
                    LittleInsight = table.Column<bool>(type: "bit", nullable: false),
                    Aware = table.Column<bool>(type: "bit", nullable: false),
                    AbleToGenerateAlternatives = table.Column<bool>(type: "bit", nullable: false),
                    Initiates = table.Column<bool>(type: "bit", nullable: false),
                    ProblemSolved = table.Column<bool>(type: "bit", nullable: false),
                    DemostratesEmpathy = table.Column<bool>(type: "bit", nullable: false),
                    UsesSessions = table.Column<bool>(type: "bit", nullable: false),
                    Variable = table.Column<bool>(type: "bit", nullable: false),
                    Setting = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    MTPId = table.Column<int>(type: "int", nullable: true),
                    Schema = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotesP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotesP_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotesP_Workdays_Clients_Workday_CientId",
                        column: x => x.Workday_CientId,
                        principalTable: "Workdays_Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotesP_Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotePId = table.Column<int>(type: "int", nullable: true),
                    ActivityId = table.Column<int>(type: "int", nullable: true),
                    Cooperative = table.Column<bool>(type: "bit", nullable: false),
                    Assertive = table.Column<bool>(type: "bit", nullable: false),
                    Passive = table.Column<bool>(type: "bit", nullable: false),
                    Variable = table.Column<bool>(type: "bit", nullable: false),
                    Uninterested = table.Column<bool>(type: "bit", nullable: false),
                    EngagedActive = table.Column<bool>(type: "bit", nullable: false),
                    Distractible = table.Column<bool>(type: "bit", nullable: false),
                    Confused = table.Column<bool>(type: "bit", nullable: false),
                    Aggresive = table.Column<bool>(type: "bit", nullable: false),
                    Resistant = table.Column<bool>(type: "bit", nullable: false),
                    Other = table.Column<bool>(type: "bit", nullable: false),
                    ObjetiveId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotesP_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotesP_Activities_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotesP_Activities_NotesP_NotePId",
                        column: x => x.NotePId,
                        principalTable: "NotesP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotesP_Activities_Objetives_ObjetiveId",
                        column: x => x.ObjetiveId,
                        principalTable: "Objetives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotesP_SupervisorId",
                table: "NotesP",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesP_Workday_CientId",
                table: "NotesP",
                column: "Workday_CientId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesP_Activities_ActivityId",
                table: "NotesP_Activities",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesP_Activities_NotePId",
                table: "NotesP_Activities",
                column: "NotePId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesP_Activities_ObjetiveId",
                table: "NotesP_Activities",
                column: "ObjetiveId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotesP_Activities");

            migrationBuilder.DropTable(
                name: "NotesP");
        }
    }
}
