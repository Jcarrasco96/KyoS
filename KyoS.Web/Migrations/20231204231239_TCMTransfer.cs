using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMTransfer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCMClientId = table.Column<int>(type: "int", nullable: true),
                    LegalGuardianName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalGuardianPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeInformation = table.Column<bool>(type: "bit", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CityStateZip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferFollow = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpeningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateServicePlanORLastSPR = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateLastService = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasClientChart = table.Column<bool>(type: "bit", nullable: false),
                    DateAudit = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateAuditSign = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TCMAssignedToId = table.Column<int>(type: "int", nullable: true),
                    OpeningDateAssignedTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TCMSupervisorId = table.Column<int>(type: "int", nullable: true),
                    TCMAssignedFromId = table.Column<int>(type: "int", nullable: true),
                    EndTransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TCMAssignedFromAccept = table.Column<bool>(type: "bit", nullable: false),
                    TCMAssignedToAccept = table.Column<bool>(type: "bit", nullable: false),
                    Return = table.Column<bool>(type: "bit", nullable: false),
                    TCMSupervisorAccept = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMTransfers_CaseManagers_TCMAssignedFromId",
                        column: x => x.TCMAssignedFromId,
                        principalTable: "CaseManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMTransfers_CaseManagers_TCMAssignedToId",
                        column: x => x.TCMAssignedToId,
                        principalTable: "CaseManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMTransfers_TCMClient_TCMClientId",
                        column: x => x.TCMClientId,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMTransfers_TCMSupervisors_TCMSupervisorId",
                        column: x => x.TCMSupervisorId,
                        principalTable: "TCMSupervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMTransfers_TCMAssignedFromId",
                table: "TCMTransfers",
                column: "TCMAssignedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMTransfers_TCMAssignedToId",
                table: "TCMTransfers",
                column: "TCMAssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMTransfers_TCMClientId",
                table: "TCMTransfers",
                column: "TCMClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMTransfers_TCMSupervisorId",
                table: "TCMTransfers",
                column: "TCMSupervisorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMTransfers");
        }
    }
}
