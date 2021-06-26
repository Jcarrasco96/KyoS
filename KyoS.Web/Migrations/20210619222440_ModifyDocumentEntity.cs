using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyDocumentEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentEntity_Clients_ClientId",
                table: "DocumentEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentEntity",
                table: "DocumentEntity");

            migrationBuilder.RenameTable(
                name: "DocumentEntity",
                newName: "Documents");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentEntity_ClientId",
                table: "Documents",
                newName: "IX_Documents_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: "Documents",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DocumentsTemp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentsTemp", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Clients_ClientId",
                table: "Documents",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Clients_ClientId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "DocumentsTemp");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: "Documents");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "DocumentEntity");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_ClientId",
                table: "DocumentEntity",
                newName: "IX_DocumentEntity_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentEntity",
                table: "DocumentEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentEntity_Clients_ClientId",
                table: "DocumentEntity",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
