using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Client_Referred1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Client_Referred_Clients_ClientId",
                table: "Client_Referred");

            migrationBuilder.DropForeignKey(
                name: "FK_Client_Referred_Referreds_ReferredId",
                table: "Client_Referred");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Client_Referred",
                table: "Client_Referred");

            migrationBuilder.DropColumn(
                name: "ReferredNote",
                table: "Referreds");

            migrationBuilder.RenameTable(
                name: "Client_Referred",
                newName: "Clients_Referreds");

            migrationBuilder.RenameIndex(
                name: "IX_Client_Referred_ReferredId",
                table: "Clients_Referreds",
                newName: "IX_Clients_Referreds_ReferredId");

            migrationBuilder.RenameIndex(
                name: "IX_Client_Referred_ClientId",
                table: "Clients_Referreds",
                newName: "IX_Clients_Referreds_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clients_Referreds",
                table: "Clients_Referreds",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ReferredsTemp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Agency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Service = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferredsTemp", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Referreds_Clients_ClientId",
                table: "Clients_Referreds",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Referreds_Referreds_ReferredId",
                table: "Clients_Referreds",
                column: "ReferredId",
                principalTable: "Referreds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Referreds_Clients_ClientId",
                table: "Clients_Referreds");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Referreds_Referreds_ReferredId",
                table: "Clients_Referreds");

            migrationBuilder.DropTable(
                name: "ReferredsTemp");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clients_Referreds",
                table: "Clients_Referreds");

            migrationBuilder.RenameTable(
                name: "Clients_Referreds",
                newName: "Client_Referred");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_Referreds_ReferredId",
                table: "Client_Referred",
                newName: "IX_Client_Referred_ReferredId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_Referreds_ClientId",
                table: "Client_Referred",
                newName: "IX_Client_Referred_ClientId");

            migrationBuilder.AddColumn<string>(
                name: "ReferredNote",
                table: "Referreds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Client_Referred",
                table: "Client_Referred",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Client_Referred_Clients_ClientId",
                table: "Client_Referred",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Client_Referred_Referreds_ReferredId",
                table: "Client_Referred",
                column: "ReferredId",
                principalTable: "Referreds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
