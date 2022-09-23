using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Client_Referred : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Referreds_ReferredId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_ReferredId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ReferredId",
                table: "Clients");

            migrationBuilder.CreateTable(
                name: "Client_Referred",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    ReferredId = table.Column<int>(type: "int", nullable: true),
                    Service = table.Column<int>(type: "int", nullable: false),
                    ReferredNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client_Referred", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Client_Referred_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Client_Referred_Referreds_ReferredId",
                        column: x => x.ReferredId,
                        principalTable: "Referreds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Client_Referred_ClientId",
                table: "Client_Referred",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Client_Referred_ReferredId",
                table: "Client_Referred",
                column: "ReferredId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Client_Referred");

            migrationBuilder.AddColumn<int>(
                name: "ReferredId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ReferredId",
                table: "Clients",
                column: "ReferredId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Referreds_ReferredId",
                table: "Clients",
                column: "ReferredId",
                principalTable: "Referreds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
