using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class CitesIndTherapy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    FacilitatorId = table.Column<int>(type: "int", nullable: true),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EventNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Copay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Worday_CLientId = table.Column<int>(type: "int", nullable: true),
                    ClinicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cite_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cite_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cite_Facilitators_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalTable: "Facilitators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cite_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cite_Workdays_Clients_Worday_CLientId",
                        column: x => x.Worday_CLientId,
                        principalTable: "Workdays_Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cite_ClientId",
                table: "Cite",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Cite_ClinicId",
                table: "Cite",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Cite_FacilitatorId",
                table: "Cite",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Cite_ScheduleId",
                table: "Cite",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Cite_Worday_CLientId",
                table: "Cite",
                column: "Worday_CLientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cite");
        }
    }
}
