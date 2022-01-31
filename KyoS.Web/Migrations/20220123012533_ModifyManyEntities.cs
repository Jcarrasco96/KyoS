using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyManyEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotesP_Workdays_Clients_Workday_CientId",
                table: "NotesP");

            migrationBuilder.DropIndex(
                name: "IX_NotesP_Workday_CientId",
                table: "NotesP");

            migrationBuilder.DropColumn(
                name: "Workday_CientId",
                table: "NotesP");

            migrationBuilder.CreateIndex(
                name: "IX_NotesP_Workday_Client_FK",
                table: "NotesP",
                column: "Workday_Client_FK",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NotesP_Workdays_Clients_Workday_Client_FK",
                table: "NotesP",
                column: "Workday_Client_FK",
                principalTable: "Workdays_Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotesP_Workdays_Clients_Workday_Client_FK",
                table: "NotesP");

            migrationBuilder.DropIndex(
                name: "IX_NotesP_Workday_Client_FK",
                table: "NotesP");

            migrationBuilder.AddColumn<int>(
                name: "Workday_CientId",
                table: "NotesP",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotesP_Workday_CientId",
                table: "NotesP",
                column: "Workday_CientId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotesP_Workdays_Clients_Workday_CientId",
                table: "NotesP",
                column: "Workday_CientId",
                principalTable: "Workdays_Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
