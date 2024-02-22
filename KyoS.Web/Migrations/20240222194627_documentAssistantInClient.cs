using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class documentAssistantInClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentsAssistantId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DocumentsAssistantId",
                table: "Clients",
                column: "DocumentsAssistantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_DocumentsAssistant_DocumentsAssistantId",
                table: "Clients",
                column: "DocumentsAssistantId",
                principalTable: "DocumentsAssistant",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_DocumentsAssistant_DocumentsAssistantId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_DocumentsAssistantId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DocumentsAssistantId",
                table: "Clients");
        }
    }
}
