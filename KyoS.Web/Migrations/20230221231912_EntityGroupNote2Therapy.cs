using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class EntityGroupNote2Therapy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupNote2EntityId",
                table: "GroupNotes_Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GroupNotes2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Workday_Client_FK = table.Column<int>(type: "int", nullable: false),
                    Workday_CientId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateOfApprove = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GroupLeaderFacilitator = table.Column<bool>(type: "bit", nullable: false),
                    GroupLeaderFacilitatorAbout = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Facilitated = table.Column<bool>(type: "bit", nullable: false),
                    Involved = table.Column<bool>(type: "bit", nullable: false),
                    Kept = table.Column<bool>(type: "bit", nullable: false),
                    GroupLeaderProviderPsychoeducation = table.Column<bool>(type: "bit", nullable: false),
                    GroupLeaderProviderSupport = table.Column<bool>(type: "bit", nullable: false),
                    Assigned = table.Column<bool>(type: "bit", nullable: false),
                    AssignedTopicOf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignificantProgress = table.Column<bool>(type: "bit", nullable: false),
                    ModerateProgress = table.Column<bool>(type: "bit", nullable: false),
                    MinimalProgress = table.Column<bool>(type: "bit", nullable: false),
                    NoProgress = table.Column<bool>(type: "bit", nullable: false),
                    Regression = table.Column<bool>(type: "bit", nullable: false),
                    Descompensating = table.Column<bool>(type: "bit", nullable: false),
                    UnableToDetermine = table.Column<bool>(type: "bit", nullable: false),
                    Oriented = table.Column<bool>(type: "bit", nullable: false),
                    NotToTime = table.Column<bool>(type: "bit", nullable: false),
                    NotToPlace = table.Column<bool>(type: "bit", nullable: false),
                    NotToPerson = table.Column<bool>(type: "bit", nullable: false),
                    Fair = table.Column<bool>(type: "bit", nullable: false),
                    InsightAdequate = table.Column<bool>(type: "bit", nullable: false),
                    Limited = table.Column<bool>(type: "bit", nullable: false),
                    Impaired = table.Column<bool>(type: "bit", nullable: false),
                    Faulty = table.Column<bool>(type: "bit", nullable: false),
                    Euthymic = table.Column<bool>(type: "bit", nullable: false),
                    Congruent = table.Column<bool>(type: "bit", nullable: false),
                    Euphoric = table.Column<bool>(type: "bit", nullable: false),
                    Optimistic = table.Column<bool>(type: "bit", nullable: false),
                    Hostile = table.Column<bool>(type: "bit", nullable: false),
                    Withdrawn = table.Column<bool>(type: "bit", nullable: false),
                    Negativistic = table.Column<bool>(type: "bit", nullable: false),
                    Depressed = table.Column<bool>(type: "bit", nullable: false),
                    Anxious = table.Column<bool>(type: "bit", nullable: false),
                    Irritable = table.Column<bool>(type: "bit", nullable: false),
                    Dramatic = table.Column<bool>(type: "bit", nullable: false),
                    Adequated = table.Column<bool>(type: "bit", nullable: false),
                    Inadequated = table.Column<bool>(type: "bit", nullable: false),
                    FairAttitude = table.Column<bool>(type: "bit", nullable: false),
                    Unmotivated = table.Column<bool>(type: "bit", nullable: false),
                    Motivated = table.Column<bool>(type: "bit", nullable: false),
                    Guarded = table.Column<bool>(type: "bit", nullable: false),
                    Normal = table.Column<bool>(type: "bit", nullable: false),
                    Short = table.Column<bool>(type: "bit", nullable: false),
                    MildlyImpaired = table.Column<bool>(type: "bit", nullable: false),
                    SevereryImpaired = table.Column<bool>(type: "bit", nullable: false),
                    Getting = table.Column<bool>(type: "bit", nullable: false),
                    Sharing = table.Column<bool>(type: "bit", nullable: false),
                    Expressing = table.Column<bool>(type: "bit", nullable: false),
                    LearningFrom = table.Column<bool>(type: "bit", nullable: false),
                    Developing = table.Column<bool>(type: "bit", nullable: false),
                    Received = table.Column<bool>(type: "bit", nullable: false),
                    Providing = table.Column<bool>(type: "bit", nullable: false),
                    LearningAbout = table.Column<bool>(type: "bit", nullable: false),
                    Other = table.Column<bool>(type: "bit", nullable: false),
                    OtherExplain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    MTPId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupNotes2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupNotes2_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupNotes2_Workdays_Clients_Workday_CientId",
                        column: x => x.Workday_CientId,
                        principalTable: "Workdays_Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupNotes2_Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupNote2Id = table.Column<int>(type: "int", nullable: true),
                    ActivityId = table.Column<int>(type: "int", nullable: true),
                    AnswerClient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnswerFacilitator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObjetiveId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupNotes2_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupNotes2_Activities_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupNotes2_Activities_GroupNotes2_GroupNote2Id",
                        column: x => x.GroupNote2Id,
                        principalTable: "GroupNotes2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupNotes2_Activities_Objetives_ObjetiveId",
                        column: x => x.ObjetiveId,
                        principalTable: "Objetives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes_Activities_GroupNote2EntityId",
                table: "GroupNotes_Activities",
                column: "GroupNote2EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes2_SupervisorId",
                table: "GroupNotes2",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes2_Workday_CientId",
                table: "GroupNotes2",
                column: "Workday_CientId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes2_Activities_ActivityId",
                table: "GroupNotes2_Activities",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes2_Activities_GroupNote2Id",
                table: "GroupNotes2_Activities",
                column: "GroupNote2Id");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes2_Activities_ObjetiveId",
                table: "GroupNotes2_Activities",
                column: "ObjetiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupNotes_Activities_GroupNotes2_GroupNote2EntityId",
                table: "GroupNotes_Activities",
                column: "GroupNote2EntityId",
                principalTable: "GroupNotes2",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupNotes_Activities_GroupNotes2_GroupNote2EntityId",
                table: "GroupNotes_Activities");

            migrationBuilder.DropTable(
                name: "GroupNotes2_Activities");

            migrationBuilder.DropTable(
                name: "GroupNotes2");

            migrationBuilder.DropIndex(
                name: "IX_GroupNotes_Activities_GroupNote2EntityId",
                table: "GroupNotes_Activities");

            migrationBuilder.DropColumn(
                name: "GroupNote2EntityId",
                table: "GroupNotes_Activities");
        }
    }
}
