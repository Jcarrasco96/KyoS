using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class SafetyPlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SafetyPlan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureClient = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureFacilitator = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Documents = table.Column<bool>(type: "bit", nullable: false),
                    FacilitatorId = table.Column<int>(type: "int", nullable: true),
                    PeopleIcanCall = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WaysToKeepmyselfSafe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdviceIwould = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WaysToDistract = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarningSignsOfCrisis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThingsThat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    DateDocument = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentAssisstantId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyPlan_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SafetyPlan_DocumentsAssistant_DocumentAssisstantId",
                        column: x => x.DocumentAssisstantId,
                        principalTable: "DocumentsAssistant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SafetyPlan_Facilitators_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalTable: "Facilitators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SafetyPlan_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SafetyPlan_Client_FK",
                table: "SafetyPlan",
                column: "Client_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SafetyPlan_DocumentAssisstantId",
                table: "SafetyPlan",
                column: "DocumentAssisstantId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyPlan_FacilitatorId",
                table: "SafetyPlan",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyPlan_SupervisorId",
                table: "SafetyPlan",
                column: "SupervisorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SafetyPlan");
        }
    }
}
