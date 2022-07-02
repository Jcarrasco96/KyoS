using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMNotesAndWorkdayTCMClientAndTCMNotesActivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Workday_TCMClientId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TCMNoteEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateOfService = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Workday_Client_FK = table.Column<int>(type: "int", nullable: false),
                    ServiceCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DocumentationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextStep = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalMinutes = table.Column<int>(type: "int", nullable: false),
                    TotalUnits = table.Column<int>(type: "int", nullable: false),
                    CaseManagerId = table.Column<int>(type: "int", nullable: true),
                    CaseManagerDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMNoteEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMNoteEntity_CaseManagers_CaseManagerId",
                        column: x => x.CaseManagerId,
                        principalTable: "CaseManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TCMNote_Activity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCMNoteId = table.Column<int>(type: "int", nullable: true),
                    Setting = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TCMDomainId = table.Column<int>(type: "int", nullable: true),
                    DescriptionOfService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Minutes = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMNote_Activity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMNote_Activity_TCMDomains_TCMDomainId",
                        column: x => x.TCMDomainId,
                        principalTable: "TCMDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMNote_Activity_TCMNoteEntity_TCMNoteId",
                        column: x => x.TCMNoteId,
                        principalTable: "TCMNoteEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workday_TCMClient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkdayId = table.Column<int>(type: "int", nullable: true),
                    TCMClientId = table.Column<int>(type: "int", nullable: true),
                    CaseManagerId = table.Column<int>(type: "int", nullable: true),
                    DateBegin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Taken = table.Column<bool>(type: "bit", nullable: false),
                    TCMNoteId = table.Column<int>(type: "int", nullable: true),
                    BilledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workday_TCMClient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workday_TCMClient_CaseManagers_CaseManagerId",
                        column: x => x.CaseManagerId,
                        principalTable: "CaseManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workday_TCMClient_TCMClient_TCMClientId",
                        column: x => x.TCMClientId,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workday_TCMClient_TCMNoteEntity_TCMNoteId",
                        column: x => x.TCMNoteId,
                        principalTable: "TCMNoteEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workday_TCMClient_Workdays_WorkdayId",
                        column: x => x.WorkdayId,
                        principalTable: "Workdays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Workday_TCMClientId",
                table: "Messages",
                column: "Workday_TCMClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMNote_Activity_TCMDomainId",
                table: "TCMNote_Activity",
                column: "TCMDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMNote_Activity_TCMNoteId",
                table: "TCMNote_Activity",
                column: "TCMNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMNoteEntity_CaseManagerId",
                table: "TCMNoteEntity",
                column: "CaseManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Workday_TCMClient_CaseManagerId",
                table: "Workday_TCMClient",
                column: "CaseManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Workday_TCMClient_TCMClientId",
                table: "Workday_TCMClient",
                column: "TCMClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Workday_TCMClient_TCMNoteId",
                table: "Workday_TCMClient",
                column: "TCMNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Workday_TCMClient_WorkdayId",
                table: "Workday_TCMClient",
                column: "WorkdayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Workday_TCMClient_Workday_TCMClientId",
                table: "Messages",
                column: "Workday_TCMClientId",
                principalTable: "Workday_TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Workday_TCMClient_Workday_TCMClientId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "TCMNote_Activity");

            migrationBuilder.DropTable(
                name: "Workday_TCMClient");

            migrationBuilder.DropTable(
                name: "TCMNoteEntity");

            migrationBuilder.DropIndex(
                name: "IX_Messages_Workday_TCMClientId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Workday_TCMClientId",
                table: "Messages");
        }
    }
}
