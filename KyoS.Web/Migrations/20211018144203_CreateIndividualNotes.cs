using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class CreateIndividualNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndividualNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Workday_Client_FK = table.Column<int>(type: "int", nullable: false),
                    SubjectiveData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObjectiveData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Assessment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlanNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateOfApprove = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Groomed = table.Column<bool>(type: "bit", nullable: false),
                    Unkempt = table.Column<bool>(type: "bit", nullable: false),
                    Disheveled = table.Column<bool>(type: "bit", nullable: false),
                    Meticulous = table.Column<bool>(type: "bit", nullable: false),
                    Overbuild = table.Column<bool>(type: "bit", nullable: false),
                    Other = table.Column<bool>(type: "bit", nullable: false),
                    Clear = table.Column<bool>(type: "bit", nullable: false),
                    Pressured = table.Column<bool>(type: "bit", nullable: false),
                    Slurred = table.Column<bool>(type: "bit", nullable: false),
                    Slow = table.Column<bool>(type: "bit", nullable: false),
                    Impaired = table.Column<bool>(type: "bit", nullable: false),
                    Poverty = table.Column<bool>(type: "bit", nullable: false),
                    Euthymic = table.Column<bool>(type: "bit", nullable: false),
                    Depressed = table.Column<bool>(type: "bit", nullable: false),
                    Anxious = table.Column<bool>(type: "bit", nullable: false),
                    Fearful = table.Column<bool>(type: "bit", nullable: false),
                    Irritable = table.Column<bool>(type: "bit", nullable: false),
                    Labile = table.Column<bool>(type: "bit", nullable: false),
                    WNL = table.Column<bool>(type: "bit", nullable: false),
                    Guarded = table.Column<bool>(type: "bit", nullable: false),
                    Withdrawn = table.Column<bool>(type: "bit", nullable: false),
                    Hostile = table.Column<bool>(type: "bit", nullable: false),
                    Restless = table.Column<bool>(type: "bit", nullable: false),
                    Impulsive = table.Column<bool>(type: "bit", nullable: false),
                    WNL_Cognition = table.Column<bool>(type: "bit", nullable: false),
                    Blocked = table.Column<bool>(type: "bit", nullable: false),
                    Obsessive = table.Column<bool>(type: "bit", nullable: false),
                    Paranoid = table.Column<bool>(type: "bit", nullable: false),
                    Scattered = table.Column<bool>(type: "bit", nullable: false),
                    Psychotic = table.Column<bool>(type: "bit", nullable: false),
                    Exceptional = table.Column<bool>(type: "bit", nullable: false),
                    Steady = table.Column<bool>(type: "bit", nullable: false),
                    Slow_Progress = table.Column<bool>(type: "bit", nullable: false),
                    Regressing = table.Column<bool>(type: "bit", nullable: false),
                    Stable = table.Column<bool>(type: "bit", nullable: false),
                    Maintain = table.Column<bool>(type: "bit", nullable: false),
                    CBT = table.Column<bool>(type: "bit", nullable: false),
                    Psychodynamic = table.Column<bool>(type: "bit", nullable: false),
                    BehaviorModification = table.Column<bool>(type: "bit", nullable: false),
                    Other_Intervention = table.Column<bool>(type: "bit", nullable: false),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    ObjectiveId = table.Column<int>(type: "int", nullable: true),
                    MTPId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndividualNotes_Objetives_ObjectiveId",
                        column: x => x.ObjectiveId,
                        principalTable: "Objetives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndividualNotes_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndividualNotes_Workdays_Clients_Workday_Client_FK",
                        column: x => x.Workday_Client_FK,
                        principalTable: "Workdays_Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndividualNotes_ObjectiveId",
                table: "IndividualNotes",
                column: "ObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualNotes_SupervisorId",
                table: "IndividualNotes",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualNotes_Workday_Client_FK",
                table: "IndividualNotes",
                column: "Workday_Client_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndividualNotes");
        }
    }
}
