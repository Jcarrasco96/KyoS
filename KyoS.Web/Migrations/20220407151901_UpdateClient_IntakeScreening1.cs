using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateClient_IntakeScreening1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeScreeningEntity_Clients_Client_FK",
                table: "IntakeScreeningEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IntakeScreeningEntity",
                table: "IntakeScreeningEntity");

            migrationBuilder.RenameTable(
                name: "IntakeScreeningEntity",
                newName: "IntakeScreenings");

            migrationBuilder.RenameIndex(
                name: "IX_IntakeScreeningEntity_Client_FK",
                table: "IntakeScreenings",
                newName: "IX_IntakeScreenings_Client_FK");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IntakeScreenings",
                table: "IntakeScreenings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeScreenings_Clients_Client_FK",
                table: "IntakeScreenings",
                column: "Client_FK",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeScreenings_Clients_Client_FK",
                table: "IntakeScreenings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IntakeScreenings",
                table: "IntakeScreenings");

            migrationBuilder.RenameTable(
                name: "IntakeScreenings",
                newName: "IntakeScreeningEntity");

            migrationBuilder.RenameIndex(
                name: "IX_IntakeScreenings_Client_FK",
                table: "IntakeScreeningEntity",
                newName: "IX_IntakeScreeningEntity_Client_FK");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IntakeScreeningEntity",
                table: "IntakeScreeningEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeScreeningEntity_Clients_Client_FK",
                table: "IntakeScreeningEntity",
                column: "Client_FK",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
