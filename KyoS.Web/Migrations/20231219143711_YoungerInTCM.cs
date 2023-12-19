using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class YoungerInTCM : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMIntakeAppendixE_TCMClient_TcmClient_FK",
                table: "TCMIntakeAppendixE");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMIntakeAppendixE_TCMSupervisors_TcmSupervisorId",
                table: "TCMIntakeAppendixE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMIntakeAppendixE",
                table: "TCMIntakeAppendixE");

            migrationBuilder.RenameTable(
                name: "TCMIntakeAppendixE",
                newName: "TCMIntakeAppendixI");

            migrationBuilder.RenameIndex(
                name: "IX_TCMIntakeAppendixE_TcmSupervisorId",
                table: "TCMIntakeAppendixI",
                newName: "IX_TCMIntakeAppendixI_TcmSupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMIntakeAppendixE_TcmClient_FK",
                table: "TCMIntakeAppendixI",
                newName: "IX_TCMIntakeAppendixI_TcmClient_FK");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMIntakeAppendixI",
                table: "TCMIntakeAppendixI",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMIntakeAppendixI_TCMClient_TcmClient_FK",
                table: "TCMIntakeAppendixI",
                column: "TcmClient_FK",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMIntakeAppendixI_TCMSupervisors_TcmSupervisorId",
                table: "TCMIntakeAppendixI",
                column: "TcmSupervisorId",
                principalTable: "TCMSupervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMIntakeAppendixI_TCMClient_TcmClient_FK",
                table: "TCMIntakeAppendixI");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMIntakeAppendixI_TCMSupervisors_TcmSupervisorId",
                table: "TCMIntakeAppendixI");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMIntakeAppendixI",
                table: "TCMIntakeAppendixI");

            migrationBuilder.RenameTable(
                name: "TCMIntakeAppendixI",
                newName: "TCMIntakeAppendixE");

            migrationBuilder.RenameIndex(
                name: "IX_TCMIntakeAppendixI_TcmSupervisorId",
                table: "TCMIntakeAppendixE",
                newName: "IX_TCMIntakeAppendixE_TcmSupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMIntakeAppendixI_TcmClient_FK",
                table: "TCMIntakeAppendixE",
                newName: "IX_TCMIntakeAppendixE_TcmClient_FK");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMIntakeAppendixE",
                table: "TCMIntakeAppendixE",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMIntakeAppendixE_TCMClient_TcmClient_FK",
                table: "TCMIntakeAppendixE",
                column: "TcmClient_FK",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMIntakeAppendixE_TCMSupervisors_TcmSupervisorId",
                table: "TCMIntakeAppendixE",
                column: "TcmSupervisorId",
                principalTable: "TCMSupervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
