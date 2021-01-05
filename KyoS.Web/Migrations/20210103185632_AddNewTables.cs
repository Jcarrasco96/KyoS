using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddNewTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Weeks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClinicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weeks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Weeks_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workdays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workdays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workdays_Weeks_WeekId",
                        column: x => x.WeekId,
                        principalTable: "Weeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workdays_Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkdayId = table.Column<int>(type: "int", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workdays_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workdays_Clients_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workdays_Clients_Workdays_WorkdayId",
                        column: x => x.WorkdayId,
                        principalTable: "Workdays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Workday_CientId = table.Column<int>(type: "int", nullable: true),
                    Plan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Workdays_Clients_Workday_CientId",
                        column: x => x.Workday_CientId,
                        principalTable: "Workdays_Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notes_Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoteId = table.Column<int>(type: "int", nullable: true),
                    ActivityId = table.Column<int>(type: "int", nullable: true),
                    AnswerClient = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AnswerFacilitator = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Activities_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notes_Activities_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Workday_CientId",
                table: "Notes",
                column: "Workday_CientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Activities_ActivityId",
                table: "Notes_Activities",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Activities_NoteId",
                table: "Notes_Activities",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Weeks_ClinicId",
                table: "Weeks",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_WeekId",
                table: "Workdays",
                column: "WeekId");

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Clients_ClientId",
                table: "Workdays_Clients",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Clients_WorkdayId",
                table: "Workdays_Clients",
                column: "WorkdayId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes_Activities");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Workdays_Clients");

            migrationBuilder.DropTable(
                name: "Workdays");

            migrationBuilder.DropTable(
                name: "Weeks");
        }
    }
}
