using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMClient_database : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TCMClientEntityId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TCMClient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CasemanagerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMClient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMClient_CaseManagers_CasemanagerId",
                        column: x => x.CasemanagerId,
                        principalTable: "CaseManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TCMClientEntityId",
                table: "Clients",
                column: "TCMClientEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMClient_CasemanagerId",
                table: "TCMClient",
                column: "CasemanagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_TCMClient_TCMClientEntityId",
                table: "Clients",
                column: "TCMClientEntityId",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_TCMClient_TCMClientEntityId",
                table: "Clients");

            migrationBuilder.DropTable(
                name: "TCMClient");

            migrationBuilder.DropIndex(
                name: "IX_Clients_TCMClientEntityId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "TCMClientEntityId",
                table: "Clients");
        }
    }
}
