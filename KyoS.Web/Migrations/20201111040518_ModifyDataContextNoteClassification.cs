using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyDataContextNoteClassification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Classifications_Notes_NoteId",
                table: "Notes_Classifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Classifications_Notes_NoteId",
                table: "Notes_Classifications",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Classifications_Notes_NoteId",
                table: "Notes_Classifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Classifications_Notes_NoteId",
                table: "Notes_Classifications",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
