using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMNotesAndWorkdayTCMClientAndTCMNotesActivity2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Workday_TCMClient_Workday_TCMClientId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNote_Activity_TCMDomains_TCMDomainId",
                table: "TCMNote_Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNote_Activity_TCMNoteEntity_TCMNoteId",
                table: "TCMNote_Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNoteEntity_CaseManagers_CaseManagerId",
                table: "TCMNoteEntity");

            migrationBuilder.DropTable(
                name: "Workday_TCMClient");

            migrationBuilder.DropIndex(
                name: "IX_Messages_Workday_TCMClientId",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMNoteEntity",
                table: "TCMNoteEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMNote_Activity",
                table: "TCMNote_Activity");

            migrationBuilder.DropColumn(
                name: "Workday_TCMClientId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Workday_Client_FK",
                table: "TCMNoteEntity");

            migrationBuilder.RenameTable(
                name: "TCMNoteEntity",
                newName: "TCMNOte");

            migrationBuilder.RenameTable(
                name: "TCMNote_Activity",
                newName: "TCMNOteActivity");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNoteEntity_CaseManagerId",
                table: "TCMNOte",
                newName: "IX_TCMNOte_CaseManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNote_Activity_TCMNoteId",
                table: "TCMNOteActivity",
                newName: "IX_TCMNOteActivity_TCMNoteId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNote_Activity_TCMDomainId",
                table: "TCMNOteActivity",
                newName: "IX_TCMNOteActivity_TCMDomainId");

            migrationBuilder.AddColumn<int>(
                name: "TCMClientId",
                table: "TCMNOte",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkdayId",
                table: "TCMNOte",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMNOte",
                table: "TCMNOte",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMNOteActivity",
                table: "TCMNOteActivity",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TCMNOte_TCMClientId",
                table: "TCMNOte",
                column: "TCMClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMNOte_WorkdayId",
                table: "TCMNOte",
                column: "WorkdayId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNOte_CaseManagers_CaseManagerId",
                table: "TCMNOte",
                column: "CaseManagerId",
                principalTable: "CaseManagers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNOte_TCMClient_TCMClientId",
                table: "TCMNOte",
                column: "TCMClientId",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNOte_Workdays_WorkdayId",
                table: "TCMNOte",
                column: "WorkdayId",
                principalTable: "Workdays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNOteActivity_TCMDomains_TCMDomainId",
                table: "TCMNOteActivity",
                column: "TCMDomainId",
                principalTable: "TCMDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNOteActivity_TCMNOte_TCMNoteId",
                table: "TCMNOteActivity",
                column: "TCMNoteId",
                principalTable: "TCMNOte",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMNOte_CaseManagers_CaseManagerId",
                table: "TCMNOte");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNOte_TCMClient_TCMClientId",
                table: "TCMNOte");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNOte_Workdays_WorkdayId",
                table: "TCMNOte");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNOteActivity_TCMDomains_TCMDomainId",
                table: "TCMNOteActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNOteActivity_TCMNOte_TCMNoteId",
                table: "TCMNOteActivity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMNOteActivity",
                table: "TCMNOteActivity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMNOte",
                table: "TCMNOte");

            migrationBuilder.DropIndex(
                name: "IX_TCMNOte_TCMClientId",
                table: "TCMNOte");

            migrationBuilder.DropIndex(
                name: "IX_TCMNOte_WorkdayId",
                table: "TCMNOte");

            migrationBuilder.DropColumn(
                name: "TCMClientId",
                table: "TCMNOte");

            migrationBuilder.DropColumn(
                name: "WorkdayId",
                table: "TCMNOte");

            migrationBuilder.RenameTable(
                name: "TCMNOteActivity",
                newName: "TCMNote_Activity");

            migrationBuilder.RenameTable(
                name: "TCMNOte",
                newName: "TCMNoteEntity");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNOteActivity_TCMNoteId",
                table: "TCMNote_Activity",
                newName: "IX_TCMNote_Activity_TCMNoteId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNOteActivity_TCMDomainId",
                table: "TCMNote_Activity",
                newName: "IX_TCMNote_Activity_TCMDomainId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNOte_CaseManagerId",
                table: "TCMNoteEntity",
                newName: "IX_TCMNoteEntity_CaseManagerId");

            migrationBuilder.AddColumn<int>(
                name: "Workday_TCMClientId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Workday_Client_FK",
                table: "TCMNoteEntity",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMNote_Activity",
                table: "TCMNote_Activity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMNoteEntity",
                table: "TCMNoteEntity",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Workday_TCMClient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BilledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CaseManagerId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateBegin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TCMClientId = table.Column<int>(type: "int", nullable: true),
                    TCMNoteId = table.Column<int>(type: "int", nullable: true),
                    Taken = table.Column<bool>(type: "bit", nullable: false),
                    WorkdayId = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNote_Activity_TCMDomains_TCMDomainId",
                table: "TCMNote_Activity",
                column: "TCMDomainId",
                principalTable: "TCMDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNote_Activity_TCMNoteEntity_TCMNoteId",
                table: "TCMNote_Activity",
                column: "TCMNoteId",
                principalTable: "TCMNoteEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNoteEntity_CaseManagers_CaseManagerId",
                table: "TCMNoteEntity",
                column: "CaseManagerId",
                principalTable: "CaseManagers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
