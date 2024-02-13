using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class IncidentReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncidentReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FacilitatorId = table.Column<int>(type: "int", nullable: true),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    DocumentAssisstantId = table.Column<int>(type: "int", nullable: true),
                    DateIncident = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateReport = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeIncident = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionIncident = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Injured = table.Column<bool>(type: "bit", nullable: false),
                    Injured_Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Witnesses = table.Column<bool>(type: "bit", nullable: false),
                    Witnesses_Contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdmissionFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentReport_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncidentReport_DocumentsAssistant_DocumentAssisstantId",
                        column: x => x.DocumentAssisstantId,
                        principalTable: "DocumentsAssistant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentReport_Facilitators_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalTable: "Facilitators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentReport_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReport_ClientId",
                table: "IncidentReport",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReport_DocumentAssisstantId",
                table: "IncidentReport",
                column: "DocumentAssisstantId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReport_FacilitatorId",
                table: "IncidentReport",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReport_SupervisorId",
                table: "IncidentReport",
                column: "SupervisorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentReport");
        }
    }
}
