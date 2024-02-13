using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddendumbyDocumentAssisstant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentAssisstantId",
                table: "Adendums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Adendums_DocumentAssisstantId",
                table: "Adendums",
                column: "DocumentAssisstantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adendums_DocumentsAssistant_DocumentAssisstantId",
                table: "Adendums",
                column: "DocumentAssisstantId",
                principalTable: "DocumentsAssistant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adendums_DocumentsAssistant_DocumentAssisstantId",
                table: "Adendums");

            migrationBuilder.DropIndex(
                name: "IX_Adendums_DocumentAssisstantId",
                table: "Adendums");

            migrationBuilder.DropColumn(
                name: "DocumentAssisstantId",
                table: "Adendums");
        }
    }
}
