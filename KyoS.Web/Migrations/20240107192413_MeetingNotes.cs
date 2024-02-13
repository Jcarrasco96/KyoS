using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class MeetingNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeetingNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupervisorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingNotes_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingNotes_Facilitators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilitatorId = table.Column<int>(type: "int", nullable: true),
                    MeetingNoteEntityId = table.Column<int>(type: "int", nullable: true),
                    Intervention = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSign = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sign = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingNotes_Facilitators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingNotes_Facilitators_Facilitators_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalTable: "Facilitators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetingNotes_Facilitators_MeetingNotes_MeetingNoteEntityId",
                        column: x => x.MeetingNoteEntityId,
                        principalTable: "MeetingNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingNotes_SupervisorId",
                table: "MeetingNotes",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingNotes_Facilitators_FacilitatorId",
                table: "MeetingNotes_Facilitators",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingNotes_Facilitators_MeetingNoteEntityId",
                table: "MeetingNotes_Facilitators",
                column: "MeetingNoteEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingNotes_Facilitators");

            migrationBuilder.DropTable(
                name: "MeetingNotes");
        }
    }
}
