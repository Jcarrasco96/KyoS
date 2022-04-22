using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class discharge_Medication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Discharge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateReport = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDischarge = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Planned = table.Column<bool>(type: "bit", nullable: false),
                    ReasonDischarge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BriefHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CourseTreatment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConditionalDischarge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralFor1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralAgency1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralContactPersonal1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralPhone1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralHoursOperation1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralFor2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralAgency2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralContactPersonal2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralPhone2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralHoursOperation2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowDischarge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentPlanObjCumpl = table.Column<bool>(type: "bit", nullable: false),
                    AgencyDischargeClient = table.Column<bool>(type: "bit", nullable: false),
                    ClientDischargeAgainst = table.Column<bool>(type: "bit", nullable: false),
                    ClientDeceased = table.Column<bool>(type: "bit", nullable: false),
                    ClientMoved = table.Column<bool>(type: "bit", nullable: false),
                    PhysicallyUnstable = table.Column<bool>(type: "bit", nullable: false),
                    ClientReferred = table.Column<bool>(type: "bit", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discharge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Discharge_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Dosage = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationEntity_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Discharge_Client_FK",
                table: "Discharge",
                column: "Client_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discharge_Id",
                table: "Discharge",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicationEntity_ClientId",
                table: "MedicationEntity",
                column: "ClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Discharge");

            migrationBuilder.DropTable(
                name: "MedicationEntity");
        }
    }
}
