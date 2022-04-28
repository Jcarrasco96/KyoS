using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class updateBio4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bio_BehavioralHistoryEntity_Bio_BioId",
                table: "Bio_BehavioralHistoryEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bio_BehavioralHistoryEntity",
                table: "Bio_BehavioralHistoryEntity");

            migrationBuilder.RenameTable(
                name: "Bio_BehavioralHistoryEntity",
                newName: "Bio_BehavioralHistory");

            migrationBuilder.RenameColumn(
                name: "BioId",
                table: "Bio_BehavioralHistory",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Bio_BehavioralHistoryEntity_BioId",
                table: "Bio_BehavioralHistory",
                newName: "IX_Bio_BehavioralHistory_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bio_BehavioralHistory",
                table: "Bio_BehavioralHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bio_BehavioralHistory_Clients_ClientId",
                table: "Bio_BehavioralHistory",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bio_BehavioralHistory_Clients_ClientId",
                table: "Bio_BehavioralHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bio_BehavioralHistory",
                table: "Bio_BehavioralHistory");

            migrationBuilder.RenameTable(
                name: "Bio_BehavioralHistory",
                newName: "Bio_BehavioralHistoryEntity");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Bio_BehavioralHistoryEntity",
                newName: "BioId");

            migrationBuilder.RenameIndex(
                name: "IX_Bio_BehavioralHistory_ClientId",
                table: "Bio_BehavioralHistoryEntity",
                newName: "IX_Bio_BehavioralHistoryEntity_BioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bio_BehavioralHistoryEntity",
                table: "Bio_BehavioralHistoryEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bio_BehavioralHistoryEntity_Bio_BioId",
                table: "Bio_BehavioralHistoryEntity",
                column: "BioId",
                principalTable: "Bio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
