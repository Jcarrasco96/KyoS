using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class caseManager_TCMsupervisor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TCMSupervisorId",
                table: "CaseManagers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseManagers_TCMSupervisorId",
                table: "CaseManagers",
                column: "TCMSupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseManagers_TCMSupervisors_TCMSupervisorId",
                table: "CaseManagers",
                column: "TCMSupervisorId",
                principalTable: "TCMSupervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseManagers_TCMSupervisors_TCMSupervisorId",
                table: "CaseManagers");

            migrationBuilder.DropIndex(
                name: "IX_CaseManagers_TCMSupervisorId",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "TCMSupervisorId",
                table: "CaseManagers");
        }
    }
}
