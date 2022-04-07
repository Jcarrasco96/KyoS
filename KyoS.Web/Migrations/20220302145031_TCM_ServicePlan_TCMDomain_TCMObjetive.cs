using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCM_ServicePlan_TCMDomain_TCMObjetive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMServicePlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDCaseManager = table.Column<int>(type: "int", nullable: false),
                    IDClient = table.Column<int>(type: "int", nullable: false),
                    DateServicePlan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CaseNumber = table.Column<int>(type: "int", nullable: false),
                    DateIntake = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateAssessment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateCertification = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Strengths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weakness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DischargerCriteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClinicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMServicePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMServicePlans_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TCMDomains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NeedsIdentified = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateIdentified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TcmServicePlanId = table.Column<int>(type: "int", nullable: true),
                    ClinicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMDomains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMDomains_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMDomains_TCMServicePlans_TcmServicePlanId",
                        column: x => x.TcmServicePlanId,
                        principalTable: "TCMServicePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TCMObjetives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Units = table.Column<int>(type: "int", nullable: false),
                    IdObjetive = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TcmDomainId = table.Column<int>(type: "int", nullable: true),
                    ClinicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMObjetives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMObjetives_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMObjetives_TCMDomains_TcmDomainId",
                        column: x => x.TcmDomainId,
                        principalTable: "TCMDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMDomains_ClinicId",
                table: "TCMDomains",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMDomains_Name",
                table: "TCMDomains",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMDomains_TcmServicePlanId",
                table: "TCMDomains",
                column: "TcmServicePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMObjetives_ClinicId",
                table: "TCMObjetives",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMObjetives_TcmDomainId",
                table: "TCMObjetives",
                column: "TcmDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlans_CaseNumber",
                table: "TCMServicePlans",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlans_ClinicId",
                table: "TCMServicePlans",
                column: "ClinicId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMObjetives");

            migrationBuilder.DropTable(
                name: "TCMDomains");

            migrationBuilder.DropTable(
                name: "TCMServicePlans");
        }
    }
}
