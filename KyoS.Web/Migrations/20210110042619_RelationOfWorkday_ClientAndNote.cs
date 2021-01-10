using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class RelationOfWorkday_ClientAndNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Workdays_Clients_Workday_CientId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_Workday_CientId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Workday_CientId",
                table: "Notes");

            migrationBuilder.AddColumn<int>(
                name: "Workday_Client_FK",
                table: "Notes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Workday_Client_FK",
                table: "Notes",
                column: "Workday_Client_FK",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Workdays_Clients_Workday_Client_FK",
                table: "Notes",
                column: "Workday_Client_FK",
                principalTable: "Workdays_Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Workdays_Clients_Workday_Client_FK",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_Workday_Client_FK",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Workday_Client_FK",
                table: "Notes");

            migrationBuilder.AddColumn<int>(
                name: "Workday_CientId",
                table: "Notes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Workday_CientId",
                table: "Notes",
                column: "Workday_CientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Workdays_Clients_Workday_CientId",
                table: "Notes",
                column: "Workday_CientId",
                principalTable: "Workdays_Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
