using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMNotesAndWorkdayTCMClientAndTCMNotesActivity4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameTable(
                name: "TCMNOteActivity",
                newName: "TCMNoteActivity");

            migrationBuilder.RenameTable(
                name: "TCMNOte",
                newName: "TCMNote");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNOteActivity_TCMNoteId",
                table: "TCMNoteActivity",
                newName: "IX_TCMNoteActivity_TCMNoteId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNOteActivity_TCMDomainId",
                table: "TCMNoteActivity",
                newName: "IX_TCMNoteActivity_TCMDomainId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNOte_WorkdayId",
                table: "TCMNote",
                newName: "IX_TCMNote_WorkdayId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNOte_TCMClientId",
                table: "TCMNote",
                newName: "IX_TCMNote_TCMClientId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNOte_CaseManagerId",
                table: "TCMNote",
                newName: "IX_TCMNote_CaseManagerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMNoteActivity",
                table: "TCMNoteActivity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMNote",
                table: "TCMNote",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNote_CaseManagers_CaseManagerId",
                table: "TCMNote",
                column: "CaseManagerId",
                principalTable: "CaseManagers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNote_TCMClient_TCMClientId",
                table: "TCMNote",
                column: "TCMClientId",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNote_Workdays_WorkdayId",
                table: "TCMNote",
                column: "WorkdayId",
                principalTable: "Workdays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNoteActivity_TCMDomains_TCMDomainId",
                table: "TCMNoteActivity",
                column: "TCMDomainId",
                principalTable: "TCMDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNoteActivity_TCMNote_TCMNoteId",
                table: "TCMNoteActivity",
                column: "TCMNoteId",
                principalTable: "TCMNote",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMNote_CaseManagers_CaseManagerId",
                table: "TCMNote");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNote_TCMClient_TCMClientId",
                table: "TCMNote");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNote_Workdays_WorkdayId",
                table: "TCMNote");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNoteActivity_TCMDomains_TCMDomainId",
                table: "TCMNoteActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNoteActivity_TCMNote_TCMNoteId",
                table: "TCMNoteActivity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMNoteActivity",
                table: "TCMNoteActivity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMNote",
                table: "TCMNote");

            migrationBuilder.RenameTable(
                name: "TCMNoteActivity",
                newName: "TCMNOteActivity");

            migrationBuilder.RenameTable(
                name: "TCMNote",
                newName: "TCMNOte");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNoteActivity_TCMNoteId",
                table: "TCMNOteActivity",
                newName: "IX_TCMNOteActivity_TCMNoteId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNoteActivity_TCMDomainId",
                table: "TCMNOteActivity",
                newName: "IX_TCMNOteActivity_TCMDomainId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNote_WorkdayId",
                table: "TCMNOte",
                newName: "IX_TCMNOte_WorkdayId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNote_TCMClientId",
                table: "TCMNOte",
                newName: "IX_TCMNOte_TCMClientId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMNote_CaseManagerId",
                table: "TCMNOte",
                newName: "IX_TCMNOte_CaseManagerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMNOteActivity",
                table: "TCMNOteActivity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMNOte",
                table: "TCMNOte",
                column: "Id");

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
    }
}
