using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMDischargeAndTCMDischargeFollowUp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMDischarge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmServicePlanId = table.Column<int>(type: "int", nullable: true),
                    StaffingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DischargeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PresentProblems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgressToward = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffSignatureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupervisorSignatureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AllServiceInPlace = table.Column<bool>(type: "bit", nullable: false),
                    Referred = table.Column<bool>(type: "bit", nullable: false),
                    ClientLeftVoluntarily = table.Column<bool>(type: "bit", nullable: false),
                    NonComplianceWithAgencyRules = table.Column<bool>(type: "bit", nullable: false),
                    ClientMovedOutArea = table.Column<bool>(type: "bit", nullable: false),
                    LackOfProgress = table.Column<bool>(type: "bit", nullable: false),
                    Other = table.Column<bool>(type: "bit", nullable: false),
                    Other_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdministrativeDischarge = table.Column<bool>(type: "bit", nullable: false),
                    AdministrativeDischarge_Explain = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMDischarge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMDischarge_TCMServicePlans_TcmServicePlanId",
                        column: x => x.TcmServicePlanId,
                        principalTable: "TCMServicePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TCMDischargeFollowUp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderAgency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextAppt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TcmDischargeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMDischargeFollowUp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMDischargeFollowUp_TCMDischarge_TcmDischargeId",
                        column: x => x.TcmDischargeId,
                        principalTable: "TCMDischarge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMDischarge_Id",
                table: "TCMDischarge",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMDischarge_TcmServicePlanId",
                table: "TCMDischarge",
                column: "TcmServicePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMDischargeFollowUp_TcmDischargeId",
                table: "TCMDischargeFollowUp",
                column: "TcmDischargeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMDischargeFollowUp");

            migrationBuilder.DropTable(
                name: "TCMDischarge");
        }
    }
}
